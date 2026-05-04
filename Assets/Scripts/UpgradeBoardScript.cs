using System;
using Scriptable_Objects;
using UnityEngine;

public class UpgradeBoardScript : MonoBehaviour
{
    public PowerUpsScript powerUpsScript;
    public UpgradeCardScript upgradeCardScript1;
    public UpgradeCardScript upgradeCardScript2;
    public UpgradeCardScript upgradeCardScript3;

    private void Start()
    {
        upgradeCardScript1.powerUpSelected.AddListener(powerUp =>
        {
            powerUpsScript.ApplyPowerUp(powerUp);
            Time.timeScale = 1;
            gameObject.SetActive(false);
        });
        
        upgradeCardScript2.powerUpSelected.AddListener(powerUp =>
        {
            powerUpsScript.ApplyPowerUp(powerUp);
            Time.timeScale = 1;
            gameObject.SetActive(false);
        });
        
        upgradeCardScript3.powerUpSelected.AddListener(powerUp =>
        {
            powerUpsScript.ApplyPowerUp(powerUp);
            Time.timeScale = 1;
            gameObject.SetActive(false);
        });
    }

    public void FillUpgradeBoard(PowerUps powerUps)
    {
        upgradeCardScript1.FillValues(powerUps.SelectRandomPowerUp());
        upgradeCardScript2.FillValues(powerUps.SelectRandomPowerUp());
        upgradeCardScript3.FillValues(powerUps.SelectRandomPowerUp());
    }
}
