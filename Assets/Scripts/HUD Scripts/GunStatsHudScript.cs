using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace HUD_Scripts
{
    public class GunStatsHudScript : MonoBehaviour
    {
        public GunStats gunStats;
        public TMP_Text fireRateText;
        public TMP_Text reloadText;
        public TMP_Text sweetSpotText;
        public TMP_Text swayText;
        public TMP_Text meleeAttackText;
        public TMP_Text meleeSpeedText;
        public TMP_Text bulletPenetrationText;
        public TMP_Text bulletSizeText;

        public void ShowValues()
        {
            var bulletScript = gunStats.bullet.GetComponent<BulletScript>();
            fireRateText.text = gunStats.fireRate.ToString();
            reloadText.text = gunStats.reloadTime.ToString();
            sweetSpotText.text = (gunStats.sweetSpotEnd - gunStats.sweetSpotStart).ToString();
            swayText.text = bulletScript.swayIntensity.ToString();
            meleeAttackText.text = gunStats.meleeAttackSize.x.ToString();
            meleeSpeedText.text = gunStats.meleeAttackDuration.ToString();
            bulletPenetrationText.text = bulletScript.maxSuperBulletPenetrations.ToString();
            bulletSizeText.text = gunStats.bulletSize.ToString();
        }
    }
}
