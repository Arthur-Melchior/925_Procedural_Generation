using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD_Scripts
{
    public class HealthBarScript : MonoBehaviour
    {
        public PlayerStats playerStats;
        public TMP_Text text;
        public Image image;

        private void Update()
        {
            text.text = playerStats.currentHealth.ToString();
            image.fillAmount = playerStats.maxHealth / playerStats.currentHealth;
        }
    }
}
