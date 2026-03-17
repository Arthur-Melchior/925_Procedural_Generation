using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class GunScript : MonoBehaviour
{
    [SerializeField] private float shootRate;
    [SerializeField] private GameObject bullet;
    
    private Transform _gunTip;
    private float _shootDeltaTime;
    private bool _isShooting;
    private Camera _camera;

    private void Start()
    {
        _gunTip = transform.Find("gun tip");
        _camera = Camera.main;
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
            var bulletInstance = Instantiate(bullet, _gunTip);
            bulletInstance.transform.SetParent(transform.parent);
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
}