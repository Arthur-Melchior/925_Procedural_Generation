using UnityEngine;

namespace HUD_Scripts
{
    public class PauseMenuScript : MonoBehaviour
    {
        public GunStatsHudScript gunStatsHudScript;
        public PlayerStatsHUD playerStatsHUD;

        public void ShowPauseMenu()
        {
            Time.timeScale = 0;
            gunStatsHudScript.gameObject.SetActive(true);
            gunStatsHudScript.ShowValues();
            playerStatsHUD.gameObject.SetActive(true);
            playerStatsHUD.ShowStats();
        }

        public void HidePauseMenu()
        {
            Time.timeScale = 1;
            gunStatsHudScript.gameObject.SetActive(false);
            playerStatsHUD.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}