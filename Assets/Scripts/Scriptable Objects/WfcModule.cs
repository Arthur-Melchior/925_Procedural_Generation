using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scriptable_Objects
{
    public enum Position
    {
        North,
        East,
        South,
        West
    }
    
    [CreateAssetMenu(fileName = "Module", menuName = "WFC", order = 0)]
    public class WfcModule : ScriptableObject
    {
        public TileBase tileBase;
        public List<WfcModuleRule> rules;
    }

    [Serializable]
    public class WfcModuleRule
    {
        public Position position;
        public List<TileBase> tiles;
    }
}