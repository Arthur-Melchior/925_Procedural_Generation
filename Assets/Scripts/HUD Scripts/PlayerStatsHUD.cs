using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace HUD_Scripts
{
    public class PlayerStatsHUD : MonoBehaviour
    {
        public PlayerStats playerStats;
        public TMP_Text healthText;
        public TMP_Text speedText;
        public TMP_Text dodgeDurationText;
        public TMP_Text dodgeForceText;
        public TMP_Text dodgeCooldownText;

        public void ShowStats()
        {
            healthText.text = playerStats.maxHealth.ToString();
            speedText.text = playerStats.speed.ToString();
            dodgeDurationText.text = playerStats.dodgeDuration.ToString();
            dodgeForceText.text = playerStats.dodgeForce.ToString();
            dodgeCooldownText.text = playerStats.dodgeCooldown.ToString();
        }
    }
}
