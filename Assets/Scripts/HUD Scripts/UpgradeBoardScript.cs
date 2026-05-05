using Scriptable_Objects;
using UnityEngine;

namespace HUD_Scripts
{
    public class UpgradeBoardScript : MonoBehaviour
    {
        public UpgradeCardScript upgradeCardScript1;
        public UpgradeCardScript upgradeCardScript2;
        public UpgradeCardScript upgradeCardScript3;

        public void FillUpgradeBoard(PowerUps powerUps)
        {
            upgradeCardScript1.FillValues(powerUps.SelectRandomPowerUp());
            upgradeCardScript2.FillValues(powerUps.SelectRandomPowerUp());
            upgradeCardScript3.FillValues(powerUps.SelectRandomPowerUp());
        }
    }
}
