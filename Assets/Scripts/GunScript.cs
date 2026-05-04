using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GunScript : MonoBehaviour
{
    [Header("Events")] [Space] public UnityEvent onReloadFinished;

    [Header("Stats")] [Tooltip("Reload time in seconds")]
    public float reloadTime = 2f;

    [Tooltip("Start of the sweet spot in seconds")]
    public float sweetSpotStart = 0.5f;

    [Tooltip("End of the sweet spot in seconds")]
    public float sweetSpotEnd = 1f;

    public int magazineSize = 20;

    [SerializeField] [Tooltip("Number of shots per second")]
    private float fireRate = 2f;

    [SerializeField] private float jamDuration = 1f;

    public float meleeAttackDuration = 0.5f;
    public Vector2 meleeAttackSize = new(2f, 2f);

    [Header("Bullet")] [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSize = 1f;
    [SerializeField] private Transform gunTip;

    [Header("HUD")] [SerializeField] private HUDScript hudScript;

    [Header("Animations")] [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField] private AnimationCurve meleeAttackCurve;
    [SerializeField] private float meleeAttackRotation = 190f;
    [SerializeField] private ParticleSystem meleeAttackVFX;

    [SerializeField] private Vector2 recoilForce = new(0.1f, 0.02f);

    [SerializeField] private Color jamColor;

    [HideInInspector] public GameObject[] bullets;
    [HideInInspector] public int remainingBullets;

    private float _shootDeltaTime;
    private bool _isShooting;
    private bool _isJammed;
    private bool _isAttacking;
    private bool _isReloading;
    private bool _reloadPressed;
    private Camera _camera;
    private ContactFilter2D _meleeAttackFilter;

    private void Start()
    {
        _camera = Camera.main;
        _meleeAttackFilter.SetLayerMask(LayerMask.GetMask("Enemy", "Bullet", "Flying Enemy"));
        bullets = new GameObject[magazineSize];
        FillMagazine();

        remainingBullets = magazineSize - 1;
    }

    private void Update()
    {
        Shoot();
    }

    private void LateUpdate()
    {
        if (!_isAttacking)
        {
            RotateGunTowardsMouse();
        }
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _isShooting = true;
        if (ctx.canceled) _isShooting = false;
    }

    public void OnMeleeAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || _isAttacking) return;
        _isAttacking = true;

        var hits = new List<Collider2D>();
        Physics2D.OverlapBox(gunTip.position, meleeAttackSize, transform.eulerAngles.z, _meleeAttackFilter, hits);

        foreach (var hit in hits)
        {
            if (hit.gameObject.CompareTag("Enemy"))
            {
                hit.gameObject.GetComponent<EnemyScript>().Die();
            }

            if (hit.gameObject.CompareTag("Bullet"))
            {
                var bulletScript = hit.gameObject.GetComponent<BulletScript>();
                bulletScript.isSuperParried = true;
                bulletScript.isSuper = true;
                bulletScript.speed *= 5;
                hit.transform.rotation = gunTip.rotation;
                hit.gameObject.GetComponent<SpriteRenderer>().color = Color.goldenRod;
            }
        }

        StartCoroutine(AttackAnimation());
    }

    private void OnDrawGizmos()
    {
        if (_isAttacking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(gunTip.position, meleeAttackSize);
        }
    }

    private IEnumerator AttackAnimation()
    {
        var delta = 0f;
        var originalRotation = spriteRenderer.transform.rotation;
        var animationDestination = Quaternion.Euler(spriteRenderer.transform.eulerAngles.x,
            spriteRenderer.transform.eulerAngles.y,
            spriteRenderer.transform.eulerAngles.z + meleeAttackRotation);
        meleeAttackVFX.transform.position = gunTip.position;
        meleeAttackVFX.Play();

        while (delta < meleeAttackDuration)
        {
            delta += Time.deltaTime;
            spriteRenderer.transform.rotation = Quaternion.Slerp(originalRotation, animationDestination,
                meleeAttackCurve.Evaluate(1 / (meleeAttackDuration / delta)));
            yield return null;
        }

        spriteRenderer.transform.rotation = transform.rotation;
        _isAttacking = false;
    }

    private void Shoot()
    {
        if (_isShooting && _shootDeltaTime > 1 / fireRate && remainingBullets > 0 && !_isReloading && !_isJammed &&
            !_isAttacking)
        {
            bullets[remainingBullets].transform.position = gunTip.position;
            bullets[remainingBullets].transform.rotation = gunTip.rotation;
            bullets[remainingBullets].transform.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
            bullets[remainingBullets].SetActive(true);
            remainingBullets--;
            StartCoroutine(Recoil());

            _shootDeltaTime = 0;
        }

        if (remainingBullets == 0 && _isShooting && !_isReloading)
        {
            StartCoroutine(Reload());
        }

        _shootDeltaTime += Time.deltaTime;
    }

    private void RotateGunTowardsMouse()
    {
        var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        var direction = mousePos - transform.parent.position;

        if (direction.sqrMagnitude > float.Epsilon)
        {
            direction.Normalize();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            transform.position = transform.parent.position + direction;
        }
    }

    public void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled) _reloadPressed = false;
        if (!ctx.performed) return;

        if (!_isReloading)
        {
            StartCoroutine(Reload());
        }
        else
        {
            _reloadPressed = true;
        }
    }

    private IEnumerator Reload()
    {
        _isReloading = true;

        var passedTime = 0f;

        while (passedTime < reloadTime)
        {
            passedTime += Time.deltaTime;
            hudScript.UpdateReloadHUD(passedTime);

            if (_reloadPressed)
            {
                if (passedTime > sweetSpotStart && passedTime < sweetSpotEnd)
                {
                    FillMagazine(true);
                }
                else
                {
                    StartCoroutine(JamGun());
                }

                yield break;
            }

            yield return null;
        }

        FillMagazine();
    }

    private IEnumerator JamGun()
    {
        onReloadFinished?.Invoke();

        _isJammed = true;
        var originalColor = spriteRenderer.color;
        spriteRenderer.color = jamColor;

        var deltaTime = Time.deltaTime;
        while (deltaTime < jamDuration)
        {
            spriteRenderer.color = Color.Lerp(jamColor, originalColor, 1 / (jamDuration / deltaTime));
            yield return null;
            deltaTime += Time.deltaTime;
        }

        spriteRenderer.color = originalColor;
        _isJammed = false;
        _isReloading = false;
        _reloadPressed = false;
    }

    private void FillMagazine(bool isSuper = false)
    {
        for (var i = 0; i < magazineSize; i++)
        {
            bullets[i] = Instantiate(bullet, gunTip);
            bullets[i].transform.SetParent(transform.parent);
            if (isSuper)
            {
                bullets[i].GetComponent<BulletScript>().isSuper = true;
            }

            bullets[i].SetActive(false);
        }

        _isReloading = false;
        _reloadPressed = false;
        remainingBullets = magazineSize - 1;
        onReloadFinished?.Invoke();
    }

    private IEnumerator Recoil()
    {
        var recoilDuration = 1 / fireRate;
        var recoilDelta = 0f;
        while (recoilDelta < recoilDuration)
        {
            var recoilPosition = new Vector3(-transform.right.x * recoilForce.x, -transform.right.y * recoilForce.y) +
                                 transform.position;
            recoilDelta += Time.deltaTime;
            spriteRenderer.transform.position =
                Vector3.Lerp(transform.position, recoilPosition, 1 / (recoilDuration / recoilDelta));
            yield return null;
        }

        spriteRenderer.transform.position = transform.position;
    }
}