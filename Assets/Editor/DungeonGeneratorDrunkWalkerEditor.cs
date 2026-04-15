using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonGeneratorDrunkWalkerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var generator = (DungeonGenerator)target;
            if (GUILayout.Button("Generate Dungeon"))
            {
               generator.GenerateDungeon();
            }
            
            if (GUILayout.Button("Generate Room"))
            {
                generator.GenerateRoom();
            }

            if (GUILayout.Button("Extend Dungeon"))
            {
                generator.ExpandRooms();
            }

            if (GUILayout.Button("Draw Walkable Tiles"))
            {
                generator.DrawWalkableTiles();
            }
        }
    }
}