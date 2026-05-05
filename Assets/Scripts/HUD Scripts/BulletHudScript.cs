using Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace HUD_Scripts
{
    public class BulletHudScript : MonoBehaviour
    {
        public GunStats gunStats;
        public GunScript gunScript;
        public TMP_Text text;
        public TMP_Text text2;

        public void Update()
        {
            text.text = gunScript.remainingBullets.ToString();
            text2.text = gunStats.magazineSize.ToString();
        }
    }
}
