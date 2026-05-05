using System;
using HUD_Scripts;
using Scriptable_Objects;
using UnityEngine;

public class PowerUpsScript : MonoBehaviour
{
    public PlayerStats playerStats;
    public GunStats gunStats;
    public PowerUps allPowerUps;
    public UpgradeBoardScript upgradeBoardScript;
    public GunStatsHudScript gunStatsHudScript;
    public PlayerStatsHUD playerStatsHUD;
    public ExperienceBarScript experienceBarScript;

    private void Start()
    {
        upgradeBoardScript.upgradeCardScript1.powerUpSelected.AddListener(OnPowerUpClick);
        upgradeBoardScript.upgradeCardScript2.powerUpSelected.AddListener(OnPowerUpClick);
        upgradeBoardScript.upgradeCardScript3.powerUpSelected.AddListener(OnPowerUpClick);
    }

    private void OnPowerUpClick(PowerUp powerUp)
    {
        ApplyPowerUp(powerUp);
        Time.timeScale = 1;
        upgradeBoardScript.gameObject.SetActive(false);
        gunStatsHudScript.gameObject.SetActive(false);
        playerStatsHUD.gameObject.SetActive(false);
        experienceBarScript.gameObject.SetActive(true);
    }

    public void ShowLevelUpMenu()
    {
        Time.timeScale = 0;
        upgradeBoardScript.gameObject.SetActive(true);
        upgradeBoardScript.FillUpgradeBoard(allPowerUps);
        gunStatsHudScript.gameObject.SetActive(true);
        gunStatsHudScript.ShowValues();
        playerStatsHUD.gameObject.SetActive(true);
        playerStatsHUD.ShowStats();
        experienceBarScript.gameObject.SetActive(false);
    }

    public void ApplyPowerUp(PowerUp? powerUp)
    {
        var bulletScript = gunStats.bullet.GetComponent<BulletScript>();
        switch (powerUp!.Value.name)
        {
            case "Fire Rate UP":
                gunStats.fireRate += 1;
                break;
            case "Magazine size UP":
                gunStats.magazineSize += 5;
                gunStats.reloadTime *= 1.05f;
                break;
            case "Bullet Penetration UP":
                bulletScript.maxSuperBulletPenetrations++;
                break;
            case "Bullet Sway DOWN":
                if (bulletScript.swayIntensity <= float.Epsilon)
                {
                    ApplyPowerUp(allPowerUps.SelectRandomPowerUp());
                    ApplyPowerUp(allPowerUps.SelectRandomPowerUp());
                }
                else
                {
                    bulletScript.swayIntensity--;
                }

                break;
            case "Speed UP":
                playerStats.speed++;
                break;
            case "Dash UP":
                playerStats.dodgeDuration += 0.2f;
                playerStats.dodgeForce++;
                break;
            case "Melee Speed UP":
                gunStats.meleeAttackDuration *= 0.9f;
                break;
            case "Melee Attack UP":
                gunStats.meleeAttackSize += Vector2.one;
                break;
            case "Sweet Spot UP":
                gunStats.sweetSpotStart *= 0.95f;
                gunStats.sweetSpotEnd *= 1.05f;
                break;
            case "Bullet Speed UP":
                bulletScript.speed += 0.5f;
                break;
            case "Health UP":
                playerStats.maxHealth *= 1.1f;
                playerStats.currentHealth *= 1.1f;
                break;
            case "Dodge Cooldown DOWN":
                if (playerStats.dodgeCooldown <= float.Epsilon)
                {
                    ApplyPowerUp(allPowerUps.SelectRandomPowerUp());
                    ApplyPowerUp(allPowerUps.SelectRandomPowerUp());
                }
                else
                {
                    playerStats.dodgeCooldown -= 0.1f;
                }

                break;
            case "Bullet size UP":
                gunStats.bulletSize *= 1.1f;
                break;
        }
    }
}