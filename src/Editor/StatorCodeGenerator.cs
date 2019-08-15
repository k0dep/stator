using System;
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

            code.AppendLine("using System;");
            code.AppendLine();

            if(builder.Namespace != null)
            {
                code.AppendLine($"namespace {builder.Namespace}");
                code.AppendLine("{");
            }

            code.AppendLine($"public partial class {builder.Name}");
            code.AppendLine("{");

            CretaeSingletons(code, builderInstance);
            
            code.AppendLine("}");
            
            if(builder.Namespace != null)
            {
                code.AppendLine("}");
            }
            
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "stator_builders"));
            File.WriteAllText(Path.Combine(Application.dataPath, "stator_builders", "builder_" + GetTypeSafeName(builder) + ".cs"), code.ToString());
        }

        private void CretaeSingletons(StringBuilder code, ContainerBuilder builderInstance)
        {
            code.AppendLine($"\t// singletons");
            var singletons = builderInstance.Registrations.Where(t => t.Lifetime == LifetimeScope.Singleton).ToArray();
            foreach (var singleton in singletons)
            {
                code.AppendLine($"\tpublic {singleton.TypeFront.GetRightFullName()} {GetSingletonName(singleton.TypeFront)};");
            }

            code.AppendLine();

            foreach (var singleton in singletons)
            {
                var fieldName = GetSingletonName(singleton.TypeFront);
                code.AppendLine($"\tpublic {singleton.TypeFront.GetRightFullName()} {GetResolveName(singleton.TypeFront)}()");
                code.AppendLine("\t{");
                code.AppendLine($"\t\tif({fieldName} == null)");
                code.AppendLine("\t\t{");

                var ctor = singleton.TypeBack.GetConstructors().OrderBy(c => c.GetParameters().Count()).First();
                foreach (var parameter in ctor.GetParameters())
                {
                    var paramType = parameter.ParameterType;
                    code.AppendLine($"\t\t\tvar {GetDependencyName(paramType)} = {GetResolveName(paramType)}();");
                }

                var paramTypes = ctor.GetParameters().Select(t => t.ParameterType).Select(GetDependencyName);
                var paramList = string.Join(", ", paramTypes);
                code.AppendLine($"\t\t\t{fieldName} = new {singleton.TypeBack.GetRightFullName()}({paramList});");

                code.AppendLine($"\t\t}}");
                code.AppendLine($"\t\treturn {fieldName};");
                code.AppendLine("\t}");
            }
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
            var name = ns + "_" +type.Name.Replace('.', '_').Replace('`', '_').Replace('*', '_');
            if (type.IsGenericType)
            {
                var paramerets = type.GetGenericArguments();
                name += "_" + string.Join("_", paramerets.Select(GetTypeSafeName).ToArray());
            }

            return name;
        }
    }
}