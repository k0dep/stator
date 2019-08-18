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

            var members = new List<CSharpClassMember>();
            members.AddRange(CreateSingletons(builderInstance));
            members.AddRange(CreateTransients(builderInstance));
            members.AddRange(CreateResolvers(builderInstance));
            members.AddRange(CreateMainResolver(builderInstance));
            members = members.OrderByDescending(m => m.GetType().ToString()).ToList();

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
                var resolveName = GetResolveNameFront(singleton.TypeFront);
                var resolveMethod = new CSharpClassMethod(singleton.TypeFront, resolveName,
                                                new MethodParameter[0], true, resolveBody);
                members.Add(resolveMethod);
            }

            return members;
        }

        private IEnumerable<CSharpClassMember> CreateTransients(ContainerBuilder builderInstance)
        {
            var members = new List<CSharpClassMember>();
            var transients = builderInstance.Registrations
                .Where(t => t.Lifetime == LifetimeScope.Transient).ToArray();

            foreach (var transient in transients)
            {
                var backResolveMethod = GetResolveName(transient.TypeBack);

                var resultStatement = new CSharpInitVariable(null, "result", new CSharpInvoke(backResolveMethod, new CSharpStatement[0]));
                var resolveBody = new CSharpStatement[]{
                    resultStatement,
                    new CSharpReturn(new CSharpSymbol("result"))
                };
                var resolveName = GetResolveNameFront(transient.TypeFront);
                var resolveMethod = new CSharpClassMethod(transient.TypeFront, resolveName,
                                                new MethodParameter[0], true, resolveBody);
                members.Add(resolveMethod);
            }

            return members;
        }

        private IEnumerable<CSharpClassMember> CreateResolvers(ContainerBuilder builderInstance)
        {
            var members = new List<CSharpClassMember>();

            foreach (var registration in builderInstance.Registrations)
            {
                var targetType = registration.TypeBack;
                var ctor = targetType.GetConstructors()
                    .OrderBy(c => c.GetParameters()
                        .Count())
                    .First();

                var statements = new List<CSharpStatement>();
                foreach (var parameter in ctor.GetParameters())
                {
                    var paramType = parameter.ParameterType;
                    var resolveInvoke = new CSharpInvoke(GetResolveNameFront(paramType), new CSharpStatement[0]);
                    var variableDependency = new CSharpInitVariable(null, GetDependencyName(paramType), resolveInvoke);
                    statements.Add(variableDependency);
                }

                var @params = ctor.GetParameters()
                    .Select(t => t.ParameterType)
                    .Select(GetDependencyName)
                    .Select(p => new CSharpSymbol(p));

                var resultVariable = new CSharpInitVariable(null, "result", new CSharpNewObject(targetType, @params));
                statements.Add(resultVariable);
                statements.Add(new CSharpReturn(new CSharpSymbol("result")));

                var resolveName = GetResolveName(targetType);
                var resolveMethod = new CSharpClassMethod(targetType, resolveName,
                                                new MethodParameter[0], true, statements);
                members.Add(resolveMethod);
            }

            return members;
        }


        private IEnumerable<CSharpClassMember> CreateMainResolver(ContainerBuilder builderInstance)
        {
            var members = new List<CSharpClassMember>();

            var field = new CSharpField(typeof(IDictionary<Type, Func<object>>), "_ResolveTable", false);
            members.Add(field);

            var initStatements = new List<CSharpStatement>();
            foreach (var registration in builderInstance.Registrations)
            {
                var leftInit = new CSharpSymbol($"_ResolveTable[typeof({registration.TypeFront.GetRightFullName()})]");
                var rightInit = new CSharpSymbol($"(Func<object>){GetResolveNameFront(registration.TypeFront)}");
                var resolverRow = new CSharpBinaryStatement(leftInit, rightInit, "=", true);
                initStatements.Add(resolverRow);
            }

            var ifStatement = new CSharpIf(new CSharpBinaryStatement(new CSharpSymbol("_ResolveTable"), CSharpSymbol.NULL, "=="), initStatements);

            var resolver = new CSharpInitVariable(null, "resolver", new CSharpSymbol("_ResolveTable[target]"));
            var resultVariable = new CSharpInitVariable(null, "result", new CSharpInvoke("resolver", new CSharpStatement[0]));
            
            var statements = new List<CSharpStatement>();
            statements.Add(ifStatement);
            statements.Add(resolver);
            statements.Add(resultVariable);
            statements.Add(new CSharpReturn(new CSharpSymbol("result")));

            var resolveMethod = new CSharpClassMethod(typeof(object), nameof(ContainerBuilder.Resolve),
                                            new MethodParameter[]{new MethodParameter(typeof(Type), "target")},
                                            true, statements, new []{"override"});
            members.Add(resolveMethod);

            return members;
        }


        public string GetSingletonName(Type type)
        {
            return $"i_{GetTypeSafeName(type)}";
        }

        public string GetResolveNameFront(Type type)
        {
            return $"F_Resolve_{GetTypeSafeName(type)}";
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