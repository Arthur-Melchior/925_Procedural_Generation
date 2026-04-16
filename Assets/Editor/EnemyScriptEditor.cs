using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomEditor(typeof(EnemyScript))]
    public class EnemyScriptEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (EnemyScript)target;

            if (GUILayout.Button("Draw Path To Target"))
            {
                script.DrawPathToTarget();
            }
        }
    }
}