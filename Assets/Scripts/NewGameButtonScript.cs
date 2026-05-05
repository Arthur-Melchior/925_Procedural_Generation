using Scriptable_Objects;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButtonScript : MonoBehaviour
{
   public GunStats defaultGunStats;
   public GunStats runtimeGunStats;
   public PlayerStats defaultPlayerStats;
   public PlayerStats runtimePlayerStats;
   public void StartNewGame()
   {
      runtimeGunStats.bullet = defaultGunStats.bullet;
      runtimeGunStats.bulletSize = defaultGunStats.bulletSize;
      runtimeGunStats.fireRate = defaultGunStats.fireRate;
      runtimeGunStats.jamDuration = defaultGunStats.jamDuration;
      runtimeGunStats.magazineSize = defaultGunStats.magazineSize;
      runtimeGunStats.meleeAttackDuration = defaultGunStats.meleeAttackDuration;
      runtimeGunStats.meleeAttackSize = defaultGunStats.meleeAttackSize;
      runtimeGunStats.reloadTime = defaultGunStats.reloadTime;
      runtimeGunStats.sweetSpotEnd = defaultGunStats.sweetSpotEnd;
      runtimeGunStats.sweetSpotStart = defaultGunStats.sweetSpotStart;

      runtimePlayerStats.dodgeCooldown = defaultPlayerStats.dodgeCooldown;
      runtimePlayerStats.dodgeDuration = defaultPlayerStats.dodgeDuration;
      runtimePlayerStats.dodgeForce = defaultPlayerStats.dodgeForce;
      runtimePlayerStats.experience = defaultPlayerStats.experience;
      runtimePlayerStats.experienceLevelUpMultiplier = defaultPlayerStats.experienceLevelUpMultiplier;
      runtimePlayerStats.experienceToLevelUp = defaultPlayerStats.experienceToLevelUp;
      runtimePlayerStats.getHitRecoil = defaultPlayerStats.getHitRecoil;
      runtimePlayerStats.maxHealth = defaultPlayerStats.maxHealth;
      runtimePlayerStats.currentHealth = defaultPlayerStats.currentHealth;
      runtimePlayerStats.invulnerabilityDuration = defaultPlayerStats.invulnerabilityDuration;
      runtimePlayerStats.speed = defaultPlayerStats.speed;
      
      SceneManager.LoadScene(1);
   }
}
