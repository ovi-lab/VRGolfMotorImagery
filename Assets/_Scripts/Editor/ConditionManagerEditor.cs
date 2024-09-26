using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityLibrary
{
    [CustomEditor(typeof(ConditionManager))]
    public class ConditionManagerEditor : Editor
    {
        private bool hasRunValidation = false;
        private bool validationResult = false;

        public override void OnInspectorGUI()
        {
            ConditionManager c = (ConditionManager)target;

            if (GUILayout.Button("Validate Solution Exists"))
            {
                validationResult = c.ValidateConstraints();
                hasRunValidation = true;
            }

            if (hasRunValidation)
            {
                string message = validationResult ? "Valid!" : "Invalid!";
                EditorGUILayout.HelpBox(message, MessageType.Info);
            }

            DrawDefaultInspector();
        }
    }
}