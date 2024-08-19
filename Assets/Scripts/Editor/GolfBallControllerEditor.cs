using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GolfBallController))]
    public class GolfBallControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GolfBallController g = (GolfBallController)target;
            if (GUILayout.Button("Re-Fire"))
            {
                g.Reset();
            }
            base.OnInspectorGUI();
        }
    }
}