using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Stator.Editor
{
    public class StatorCodeGenerator
    {
        public void Generate()
        {
            var builderType = typeof(ContainerBuilder);
            var builders = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => builderType.IsAssignableFrom(t))
                .Where(t => t != builderType)
                .ToArray();

            foreach (var builder in builders)
            {
                GenerateForBuilder(builder);
            }
        }

        private void GenerateForBuilder(Type builder)
        {
            var builderInstance = Activator.CreateInstance(builder) as ContainerBuilder;
            if (builderInstance == null)
            {
                Debug.Log($"Cant create builde for type {builder}");
                return;
            }

            var code = new StringBuilder();
            var codeBuilder = new IndentedStringBuilder(code, 0);

            var members = CreateSingletons(builderInstance);

            var className = builder.Name;
            var type = new CSharpClass(className, members, true);
            var ns = new CSharpNamespace(builder.Namespace, new CSharpClass[] { type });
            var file = new CSharpFile(new string[] { "System" }, new CSharpNamespace[] { ns });

            file.Generate(codeBuilder);

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "stator_builders"));
            File.WriteAllText(Path.Combine(Application.dataPath, "stator_builders", "builder_" + GetTypeSafeName(builder) + ".cs"), code.ToString());
        }

        private IEnumerable<CSharpClassMember> CreateSingletons(ContainerBuilder builderInstance)
        {
            var members = new List<CSharpClassMember>();
            var singletons = builderInstance.Registrations
                .Where(t => t.Lifetime == LifetimeScope.Singleton).ToArray();

            foreach (var singleton in singletons)
            {
                var fieldName = GetSingletonName(singleton.TypeFront);
                var field = new CSharpField(singleton.TypeFront, fieldName, false);
                members.Add(field);

                var backResolveMethod = GetResolveName(singleton.TypeBack);
                var condition = new CSharpBinaryStatement(new CSharpSymbol(fieldName), CSharpSymbol.NULL, "==");
                var statements = new CSharpStatement[] {
                    new CSharpBinaryStatement(new CSharpSymbol(fieldName), new CSharpInvoke(backResolveMethod, new CSharpStatement[0]), "=", true),
                };
                var ifStatement = new CSharpIf(condition, statements);
                var resolveBody = new CSharpStatement[]{
                    ifStatement,
                    new CSharpReturn(new CSharpSymbol(fieldName))
                };
                var resolveName = GetResolveName(singleton.TypeFront);
                var resolveMethod = new CSharpClassMethod(singleton.TypeFront, resolveName,
                                                new MethodParameter[0], true, resolveBody);
                members.Add(resolveMethod);
            }

            return members;

            // foreach (var singleton in singletons)
            // {
            //     var ctor = singleton.TypeBack.GetConstructors().OrderBy(c => c.GetParameters().Count()).First();
            //     foreach (var parameter in ctor.GetParameters())
            //     {
            //         var paramType = parameter.ParameterType;
            //         code.AppendLine($"\t\t\tvar {GetDependencyName(paramType)} = {GetResolveName(paramType)}();");
            //     }

            //     var paramTypes = ctor.GetParameters().Select(t => t.ParameterType).Select(GetDependencyName);
            //     var paramList = string.Join(", ", paramTypes);
            //     code.AppendLine($"\t\t\t{fieldName} = new {singleton.TypeBack.GetRightFullName()}({paramList});");

            //     code.AppendLine($"\t\t}}");
            //     code.AppendLine($"\t\treturn {fieldName};");
            //     code.AppendLine("\t}");
            // }
        }

        public string GetSingletonName(Type type)
        {
            return $"i_{GetTypeSafeName(type)}";
        }

        public string GetResolveName(Type type)
        {
            return $"Resolve_{GetTypeSafeName(type)}";
        }

        public string GetDependencyName(Type type)
        {
            return $"dep_{GetTypeSafeName(type)}";
        }

        public string GetTypeSafeName(Type type)
        {
            var ns = type.Namespace?.GetHashCode().ToString("X");
            var name = ns + "_" + type.Name.Replace('.', '_').Replace('`', '_').Replace('*', '_');
            if (type.IsGenericType)
            {
                var paramerets = type.GetGenericArguments();
                name += "_" + string.Join("_", paramerets.Select(GetTypeSafeName).ToArray());
            }

            return name;
        }
    }
}