using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class GunScript : MonoBehaviour
{
    [SerializeField] private float shootRate;
    [SerializeField] private float reloadTime;
    [SerializeField] private float sweetSpotStart;
    [SerializeField] private float sweetSpotEnd;
    [SerializeField] private int magazineSize = 20;
    [SerializeField] private GameObject bullet;

    [HideInInspector] public GameObject[] bullets;
    [HideInInspector] public int remainingBullets;

    [Header("UI")] [SerializeField] private Image backgroundImage;
    [SerializeField] private Image forGroundImage;
    [SerializeField] private Image sweetSpotImage;
    [SerializeField] private Transform reloadHud;
    [SerializeField] private Transform bulletHud;
    [SerializeField] private Image bulletSprite;

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
            Instantiate(bulletSprite, bulletHud, true);
        }

        remainingBullets = magazineSize - 1;
        sweetSpotImage.transform.rotation = Quaternion.Euler(0, 0, 360 / reloadTime * sweetSpotStart * -1);
        sweetSpotImage.fillAmount = 1 / (reloadTime / (sweetSpotEnd - sweetSpotStart));
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
        if (_isShooting && _shootDeltaTime > shootRate)
        {
            if (remainingBullets > 0)
            {
                bullets[remainingBullets].transform.position = _gunTip.position;
                bullets[remainingBullets].transform.rotation = _gunTip.rotation;
                bullets[remainingBullets].SetActive(true);
                bullets[remainingBullets].transform.SetParent(transform);
                remainingBullets--;
            }
            else
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
        reloadHud.gameObject.SetActive(true);
        var passedTime = 0f;

        while (passedTime < reloadTime)
        {
            passedTime += Time.deltaTime;
            forGroundImage.fillAmount = 1 / reloadTime * passedTime;

            if (_reloadPressed)
            {
                if (passedTime > sweetSpotStart && passedTime < sweetSpotEnd)
                {
                    for (var i = 0; i < magazineSize; i++)
                    {
                        bullets[i] = Instantiate(bullet, _gunTip);
                        bullets[i].transform.SetParent(transform.parent);
                        bullets[i].GetComponent<BulletScript>().isSuper = true;
                        bullets[i].SetActive(false);
                    }

                    _isReloading = false;
                    _reloadPressed = false;
                    remainingBullets = magazineSize - 1;
                    reloadHud.gameObject.SetActive(false);
                    yield break;
                }
                else
                {
                    //jam gun
                }
            }

            yield return null;
        }

        for (var i = 0; i < magazineSize; i++)
        {
            bullets[i] = Instantiate(bullet, _gunTip);
            bullets[i].transform.SetParent(transform.parent);
            bullets[i].SetActive(false);
        }

        _isReloading = false;
        _reloadPressed = false;
        remainingBullets = magazineSize - 1;
        reloadHud.gameObject.SetActive(false);
    }
}