using System;
using Scriptable_Objects;
using UnityEngine;
using Random = UnityEngine.Random;

public class PowerUpsScript : MonoBehaviour
{
    public PlayerScript playerScript;
    public GunScript gunScript;
    public PlayerStats playerStats;
    public GunStats gunStats;
    public PowerUps allPowerUps;
    public UpgradeBoardScript upgradeBoardScript;

    private PlayerStats _runTimePlayerStats;
    private GunStats _runTimeGunStats;

    private void Start()
    {
        _runTimePlayerStats = Instantiate(playerStats);
        _runTimeGunStats = Instantiate(gunStats);
        playerScript.playerStats = _runTimePlayerStats;
        gunScript.gunStats = _runTimeGunStats;
    }

    public void ShowLevelUpMenu()
    {
        Time.timeScale = 0;
        upgradeBoardScript.gameObject.SetActive(true);
        upgradeBoardScript.FillUpgradeBoard(allPowerUps);
    }

    public void ApplyPowerUp(PowerUp? powerUp)
    {
        var bulletScript = _runTimeGunStats.bullet.GetComponent<BulletScript>();
        switch (powerUp!.Value.name)
        {
            case "Fire Rate UP":
                _runTimeGunStats.fireRate += 1;
                break;
            case "Magazine size UP":
                _runTimeGunStats.magazineSize += 5;
                _runTimeGunStats.reloadTime *= 1.05f;
                break;
            case "Bullet Penetration UP":
                bulletScript.maxSuperBulletPenetrations++;
                break;
            case "Bullet Sway DOWN":
                if (bulletScript.swayIntensity == 0)
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
                _runTimePlayerStats.speed++;
                break;
            case "Dash UP":
                _runTimePlayerStats.dodgeDuration += 0.2f;
                _runTimePlayerStats.dodgeForce++;
                break;
            case "Melee Speed UP":
                _runTimeGunStats.meleeAttackDuration *= 0.9f;
                break;
            case "Melee Attack UP":
                _runTimeGunStats.meleeAttackSize += Vector2.one;
                break;
            case "Sweet Spot UP":
                _runTimeGunStats.sweetSpotStart *= 0.95f;
                _runTimeGunStats.sweetSpotEnd *= 1.05f;
                break;
            case "Bullet Speed UP":
                bulletScript.speed += 0.5f;
                break;
            case "Health UP":
                _runTimePlayerStats.health *= 1.1f;
                break;
            case "Dodge Cooldown DOWN":
                if (_runTimePlayerStats.dodgeCooldown == 0)
                {
                    ApplyPowerUp(allPowerUps.SelectRandomPowerUp());
                    ApplyPowerUp(allPowerUps.SelectRandomPowerUp());
                }
                else
                {
                    _runTimePlayerStats.dodgeCooldown -= 0.1f;
                }

                break;
            case "Bullet size UP":
                _runTimeGunStats.bulletSize *= 1.1f;
                break;
        }
    }
}