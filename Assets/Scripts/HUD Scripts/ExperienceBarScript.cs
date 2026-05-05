using Scriptable_Objects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HUD_Scripts
{
    public class ExperienceBarScript : MonoBehaviour
    {
        public PlayerStats playerStats;
        public TMP_Text currentXP;
        public TMP_Text NextXP;
        public Image image;

        public void Update()
        {
            currentXP.text = playerStats.experience.ToString();
            NextXP.text = playerStats.experienceToLevelUp.ToString();
            image.fillAmount = playerStats.experienceToLevelUp / playerStats.experience;
        }
    }
}
