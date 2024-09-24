using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrialHandler))]
public class TrailHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TrialHandler t = (TrialHandler)target;
        if (GUILayout.Button("Misfire Ball"))
        {
            t.Misfire();
        }

        DrawDefaultInspector();
    }
}
