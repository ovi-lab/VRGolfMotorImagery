﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityLibrary
{
    [CustomEditor(typeof(GolfBallController))]
    public class GolfBallControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GolfBallController g = (GolfBallController)target;
            if (GUILayout.Button("Fire Ball"))
            {
                g.FireBall();
            }

            if (GUILayout.Button("Reset Ball"))
            {
                g.ResetBall();
            }

            DrawDefaultInspector();
        }
    }
}