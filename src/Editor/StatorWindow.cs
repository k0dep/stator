using UnityEngine;
using UnityEditor;
using System.IO;

namespace Stator.Editor
{
    public class StatorWindow : EditorWindow
    {
        [MenuItem("Window/Stator/Options")]
        public static void OpenWindow()
        {
            var window = EditorWindow.GetWindow<StatorWindow>();
            window.titleContent = new GUIContent("Stator options");
            window.Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Clean auto generated"))
            {
                Clean();
            }

            if (GUILayout.Button("Force generate"))
            {
                Generate();
            }
        }

        private void Generate()
        {
            var service = new StatorRefreshService(new StatorSettings());
            service.Refresh();
        }

        private void Clean()
        {
            var settings = new StatorSettings();
            Directory.Delete(settings.PathToGenerate, true);
            AssetDatabase.Refresh();
        }
    }
} 