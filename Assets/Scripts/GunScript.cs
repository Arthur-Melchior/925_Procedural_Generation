using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class GunScript : MonoBehaviour
{
    public UnityEvent onReloadFinished;
    public float reloadTime;
    public float sweetSpotStart;
    public float sweetSpotEnd;
    public int magazineSize = 20;

    [SerializeField] private HUDScript hudScript;
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject bullet;

    [HideInInspector] public GameObject[] bullets;
    [HideInInspector] public int remainingBullets;

    private Transform _gunTip;
    private float _shootDeltaTime;
    private bool _isShooting;
    private Camera _camera;

    private void Start()
    {
        _gunTip = transform.Find("gun tip");
        _camera = Camera.main;
        bullets = new GameObject[magazineSize];
        for (var i = 0; i < magazineSize; i++)
        {
            bullets[i] = Instantiate(bullet, _gunTip);
            bullets[i].transform.SetParent(transform.parent);
            bullets[i].GetComponent<BulletScript>().isSuper = true;
            bullets[i].SetActive(false);
        }

        remainingBullets = magazineSize - 1;
    }

    private void Update()
    {
        Shoot();
        RotateGunTowardsMouse();
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) _isShooting = true;
        if (ctx.canceled) _isShooting = false;
    }

    private void Shoot()
    {
        if (_isShooting && _shootDeltaTime > 1 / fireRate && remainingBullets > 0)
        {
            bullets[remainingBullets].transform.position = _gunTip.position;
            bullets[remainingBullets].transform.rotation = _gunTip.rotation;
            bullets[remainingBullets].SetActive(true);
            bullets[remainingBullets].transform.SetParent(transform);
            remainingBullets--;

            if (remainingBullets == 0)
            {
                StartCoroutine(Reload());
            }
            
            _shootDeltaTime = 0;
        }

        _shootDeltaTime += Time.deltaTime;
    }

    private void RotateGunTowardsMouse()
    {
        var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        var direction = mousePos - transform.position;

        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
    }

    private bool _isReloading;
    private bool _reloadPressed;

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
                    yield break;
                }
                else
                {
                    //jam gun
                }
            }

            yield return null;
        }

        FillMagazine();
    }

    private void FillMagazine(bool isSuper = false)
    {
        for (var i = 0; i < magazineSize; i++)
        {
            bullets[i] = Instantiate(bullet, _gunTip);
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
}