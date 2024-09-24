using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GolfBallController))]
public class GolfBallControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GolfBallController g = (GolfBallController)target;
        if (GUILayout.Button("Fire Ball"))
        {
            g.FireBall(g.HoleTransform.position);
        }

        if (GUILayout.Button("Reset Ball"))
        {
            g.ResetBall();
        }

        DrawDefaultInspector();
    }
}
