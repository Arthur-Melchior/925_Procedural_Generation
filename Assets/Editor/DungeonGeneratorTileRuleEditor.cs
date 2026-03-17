using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DungeonGeneratorTileRule))]
    public class DungeonGeneratorTileRuleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (DungeonGeneratorTileRule) target;

            if (GUILayout.Button("Generate Dungeon"))
            {
                generator.GenerateDungeon();
            }
        }
    }
}
