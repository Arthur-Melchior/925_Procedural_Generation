using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomEditor(typeof(EnemiesManager))]
    public class EnemiesManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (EnemiesManager)target;

            if (GUILayout.Button("Draw Path To Target"))
            {
                script.DrawPathToTarget();
            }
        }
    }
}