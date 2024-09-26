using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityLibrary
{
    [CustomEditor(typeof(InputHandler))]
    public class InputHandlerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            InputHandler i = (InputHandler)target;
            if (GUILayout.Button("Simulate Input"))
            {
                i.SimulateInput();
            }
            DrawDefaultInspector();
        }
    }
}