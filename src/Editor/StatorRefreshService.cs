﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Stator.Editor
{
    public class StatorRefreshService
    {
        public readonly StatorSettings Settings;

        public StatorRefreshService(StatorSettings settings)
        {
            Settings = settings;
        }

        [InitializeOnLoadMethod]
        public static void Init()
        {
            var settings = new StatorSettings();
            var service = new StatorRefreshService(settings);

            if (settings.DisabledAutoRefreshing)
            {
                service.DebugInfo("Auto refreshing disabled");
                return;
            }

            CompilationPipeline.assemblyCompilationFinished += service.OnFinishedCompilation;
            service.Refresh();
        }

        private void OnFinishedCompilation(string assembly, CompilerMessage[] messages)
        {
            DebugInfo($"Begin check for problem generated files");
            foreach (var message in messages)
            {
                if (message.type != CompilerMessageType.Error)
                {
                    continue;
                }

                if (!message.file.Contains(Settings.AutoGeneratedFolder + "/builder_"))
                {
                    continue;
                }

                Info($"Removed problem generated file at path {message.file}");

                File.Delete(message.file);
            }

            DebugInfo($"Finish check for problem generated files");
        }

        public void Clean()
        {
            var asmdefFolders = GetAsmDefToFolder();
            var allFiles = GetAllGeneratedFiles(asmdefFolders);
            foreach (var file in allFiles)
            {
                File.Delete(file);
                DebugInfo($"File `{file}` deleted");
            }
        }

        public void Refresh()
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

            var asmdefFolders = GetAsmDefToFolder();
            var files = Generate(generator, factoryTypes, asmdefFolders);

            var allFiles = GetAllGeneratedFiles(asmdefFolders);
            var fileForDelete = allFiles.Except(files);
            foreach (var deletingFile in fileForDelete)
            {
                Info($"Remove old file {deletingFile}");
                File.Delete(deletingFile);
            }

            AssetDatabase.Refresh();

            DebugInfo($"Finish generating code");
        }

        private List<string> Generate(StatorCodeGenerator generator, Type[] factoryTypes,
            Dictionary<string, string> asmdefFolders)
        {
            var files = new List<string>();
            DebugInfo("Begin refreshing auto generated files");
            foreach (var factoryType in factoryTypes)
            {
                DebugInfo($"Begin generate code for `{factoryType}` type");
                var code = generator.Generate(factoryType);
                var path = Path.Combine(Settings.PathToGenerate, "builder_" + factoryType.GetTypeSafeName() + ".cs");
                var assemblyName = factoryType.Assembly.GetName().Name;
                if (asmdefFolders.TryGetValue(assemblyName, out var asmPath))
                {
                    path = Path.Combine(asmPath, Settings.AutoGeneratedFolder,
                        "builder_" + factoryType.GetTypeSafeName() + ".cs");
                }
                else
                {
                    DebugInfo($"Not found asmdef folder for assembly {assemblyName}");
                }

                Directory.CreateDirectory(Path.GetDirectoryName(path));

                DebugInfo($"Generated code for `{factoryType}` type, file: {path}");
                files.Add(path);
                if (File.Exists(path))
                {
                    var oldCode = File.ReadAllText(path);
                    if (!string.Equals(code, oldCode))
                    {
                        Info($"Code for `{factoryType}` type will rewrite");
                        File.WriteAllText(path, code);
                    }
                    else
                    {
                        DebugInfo($"Generated code for `{factoryType}` type equal old code. Skippig.");
                    }
                }
                else
                {
                    Info($"Code for `{factoryType}` type will write in file: {path}");
                    File.WriteAllText(path, code);
                }
            }

            return files;
        }

        private void DebugInfo(string info)
        {
            if (Settings.EnableDebugging)
            {
                Debug.Log("<color=blue>[STATOR:DEBUG]</color> " + info);
            }
        }

        private void Info(string info)
        {
            Debug.Log("<color=blue>[STATOR:INFO]</color> " + info);
        }

        private Dictionary<string, string> GetAsmDefToFolder()
        {
            return AssetDatabase.FindAssets($"t:asmdef")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(g => new
                    {path = Path.GetDirectoryName(g), data = JsonUtility.FromJson<AsmdefFile>(File.ReadAllText(g))})
                .ToDictionary(g => g.data.name, g => g.path);
        }

        private string[] GetAllGeneratedFiles(Dictionary<string, string> asmdefFolders)
        {
            return asmdefFolders.Values.Union(new[] {Path.GetFullPath("Assets/")})
                .Select(f => Path.Combine(f, Settings.AutoGeneratedFolder))
                .Where(t => Directory.Exists(t))
                .SelectMany(f => Directory.GetFiles(f, "*.cs"))
                .ToArray();
        }
    }

    [Serializable]
    internal class AsmdefFile
    {
        public string name;
    }
}