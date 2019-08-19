using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System.Collections.Generic;

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
            var validators = new Dictionary<Type, IRegistrationValidator>()
            {
                [typeof(ContainerRegistrationDirect)] = new DirectRegistrationValidator(),
                [typeof(ContainerRegistrationMethod)] = new MethodRegistrationValidator()
            };

            var generators = new Dictionary<Type, ICodeRegistrationGenerator>()
            {
                [typeof(ContainerRegistrationDirect)] = new DirectCodeRegistrationGenerator(),
                [typeof(ContainerRegistrationMethod)] = new MethodCodeRegistrationGenerator()
            };

            var validator = new ContainerDependencyValidator(validators);
            var generator = new StatorCodeGenerator(validator, generators);

            var factoryBaseType = typeof(ContainerFactory);
            var factoryTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => factoryBaseType.IsAssignableFrom(t))
                .Where(t => t != factoryBaseType)
                .ToArray();
            
            var pathToGeneration = Path.Combine(Application.dataPath, "stator_builders");
            Directory.Delete(pathToGeneration, true);
            foreach (var factoryType in factoryTypes)
            {
                var code = generator.Generate(factoryType);
                Directory.CreateDirectory(pathToGeneration);
                File.WriteAllText(Path.Combine(pathToGeneration, "builder_" + factoryType.GetTypeSafeName() + ".cs"), code);
            }

            AssetDatabase.Refresh();
        }
    }
}