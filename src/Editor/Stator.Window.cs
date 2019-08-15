using System;
using UnityEngine;
using UnityEditor;


namespace Stator.Editor
{
    public class StatorWindow : EditorWindow
    {
        [MenuItem("Window/Stator/Stator window")]
        public static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<StatorWindow>();
            window.titleContent = new GUIContent("Stator");
            window.Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Generate"))
            {
                var generator = new StatorCodeGenerator();
                generator.Generate();
            }
        }
    }
}