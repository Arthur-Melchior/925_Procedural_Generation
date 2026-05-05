using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
    
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public float experience;
        public float experienceToLevelUp = 100f;
        public float experienceLevelUpMultiplier = 1f;
        public float speed = 3f;
        public float invulnerabilityDuration = 1f;
        public float dodgeForce = 2f;
        public float dodgeDuration = 0.2f;
        public float dodgeCooldown = 1f;
        public float getHitRecoil = 2f;
    }
}
