using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "PowerUps", menuName = "Scriptable Objects/PowerUps")]
    public class PowerUps : ScriptableObject
    {
        public PowerUp[] powerUps;

        public PowerUp? SelectRandomPowerUp()
        {
            var randomValue = Random.value;
            var spawnRatio = 1 / powerUps.Sum(p => p.spawnForce);

            var probSum = 0f;
            foreach (var powerUp in powerUps)
            {
                var probability = powerUp.spawnForce * spawnRatio;
                if (randomValue <= probability + probSum)
                {
                    return powerUp;
                }

                probSum += probability;
            }

            return null;
        }
    }

    [Serializable]
    public struct PowerUp
    {
        public string name;
        public string description;
        public Sprite sprite;
        public float spawnForce;
    }
}
