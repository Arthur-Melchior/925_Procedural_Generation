using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(WfcMapGenerator))]
    public class WfcMapGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(20);
            if (GUILayout.Button("Fill map"))
            {
                var t = (WfcMapGenerator)target;
                t.FillMap();
            }
        }
    }
}