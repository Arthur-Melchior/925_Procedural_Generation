using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "GunStats", menuName = "Scriptable Objects/GunStats")]
    public class GunStats : ScriptableObject
    {
        [Header("Stats")] [Tooltip("Reload time in seconds")]
        public float reloadTime = 2f;

        [Tooltip("Start of the sweet spot in seconds")]
        public float sweetSpotStart = 0.5f;

        [Tooltip("End of the sweet spot in seconds")]
        public float sweetSpotEnd = 1f;

        public int magazineSize = 20;

        [Tooltip("Number of shots per second")]
        public float fireRate = 2f;

        public float jamDuration = 1f;

        public float meleeAttackDuration = 0.5f;
        public Vector2 meleeAttackSize = new(2f, 2f);

        [Header("Bullet")] 
        public GameObject bullet;
        public float bulletSize = 1f;
    }
}
