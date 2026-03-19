using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DungeonGeneratorDrunkWalker))]
    public class DungeonGeneratorDrunkWalkerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (DungeonGeneratorDrunkWalker)target;
            if (GUILayout.Button("Generate Dungeon"))
            {
               generator.GenerateDungeon();
            }
            
            if (GUILayout.Button("Generate Room"))
            {
                generator.MakeRoom();
            }

            if (GUILayout.Button("Extend Dungeon"))
            {
                generator.ExpandRooms();
            }
            
            
        }
    }
}