using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

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
                Generate();
            }
        }

        private void Generate()
        {
            var validator = new ContainerDependencyValidator();
            var generator = new StatorCodeGenerator(validator);

            var factoryBaseType = typeof(ContainerFactory);
            var factoryTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => factoryBaseType.IsAssignableFrom(t))
                .Where(t => t != factoryBaseType)
                .ToArray();
            
            foreach (var factoryType in factoryTypes)
            {
                var code = generator.Generate(factoryType);
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "stator_builders"));
                File.WriteAllText(Path.Combine(Application.dataPath, "stator_builders", "builder_" + factoryType.GetTypeSafeName() + ".cs"), code);
            }
        }
    }
}