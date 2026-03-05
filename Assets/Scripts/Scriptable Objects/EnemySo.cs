using UnityEngine;

namespace Scriptable_Objects
{
    [CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
    public class EnemySo : ScriptableObject
    {
        public GameObject prefab;
        public float cost;
    }
}
