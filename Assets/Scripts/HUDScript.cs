using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDScript : MonoBehaviour
{
    public GunScript gunScript;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image forGroundImage;
    [SerializeField] private Image sweetSpotImage;
    [SerializeField] private Transform reloadHud;
    [SerializeField] private Transform bulletHud;
    [SerializeField] private Sprite bulletSprite;
    [SerializeField] private Vector3 reloadHudOffset;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
        gunScript.onReloadFinished.AddListener(() => reloadHud.gameObject.SetActive(false));
        sweetSpotImage.transform.rotation =
            Quaternion.Euler(0, 0, 360 / gunScript.reloadTime * gunScript.sweetSpotStart * -1);
    }

    public void UpdateReloadHUD(float passedTime)
    {
        reloadHud.gameObject.SetActive(true);
        sweetSpotImage.fillAmount = 1 / (gunScript.reloadTime / (gunScript.sweetSpotEnd - gunScript.sweetSpotStart));
        forGroundImage.fillAmount = 1 / gunScript.reloadTime * passedTime;
        reloadHud.position = _camera.WorldToScreenPoint(gunScript.transform.position + reloadHudOffset);
    }
}