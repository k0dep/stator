using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stator.Editor
{
    public class StatorCodeGenerator
    {
        public ContainerDependencyValidator Validator { get; set; }

        public StatorCodeGenerator(ContainerDependencyValidator validator)
        {
            Validator = validator;
        }

        public string Generate(Type factory)
        {
            if (!typeof(ContainerFactory).IsAssignableFrom(factory))
            {
                throw new ArgumentException($"Type {factory} not inherits from {nameof(ContainerFactory)}");
            }

            var builderInstance = Activator.CreateInstance(factory) as ContainerFactory;
            if (builderInstance == null)
            {
                throw new ArgumentException($"Cant create factory for factorytype {factory}. Missing default constructor.");
            }

            CheckDependencies(builderInstance);

            return GenerateFactory(factory, builderInstance);
        }

        public void CheckDependencies(ContainerFactory instance)
        {
            var errors = new List<string>();
            if (!Validator.Validate(instance, errors))
            {
                throw new ArgumentException("Generation error:\n" + string.Join("\n", errors));
            }
        }

        private string GenerateFactory(Type factoryType, ContainerFactory instance)
        {
            var code = new StringBuilder();
            var codeBuilder = new IndentedStringBuilder(code, 0);

            var members = new List<CSharpClassMember>();
            members.AddRange(CreateSingletons(instance));
            members.AddRange(CreateTransients(instance));
            members.AddRange(CreateResolvers(instance));
            members.AddRange(CreateMainResolver(instance));
            members = members.OrderByDescending(m => m.GetType().ToString()).ToList();

            var className = factoryType.Name;
            var type = new CSharpClass(className, members, true);
            var ns = new CSharpNamespace(factoryType.Namespace, new CSharpClass[] { type });
            var file = new CSharpFile(new string[] { "System" }, new CSharpNamespace[] { ns });

            file.Generate(codeBuilder);

            return codeBuilder.ToString();
        }

        private IEnumerable<CSharpClassMember> CreateSingletons(ContainerFactory builderInstance)
        {
            var members = new List<CSharpClassMember>();
            var singletons = builderInstance.Registrations
                .Where(t => t.Lifetime == LifetimeScope.Singleton).ToArray();

            foreach (var singleton in singletons)
            {
                var fieldName = GetSingletonName(singleton.Binding);
                var field = new CSharpField(singleton.Binding, fieldName, false);
                members.Add(field);

                var backResolveMethod = GetResolveName(singleton.Implementation);
                var condition = new CSharpBinaryStatement(new CSharpSymbol(fieldName), CSharpSymbol.NULL, "==");
                var statements = new CSharpStatement[] {
                    new CSharpBinaryStatement(new CSharpSymbol(fieldName), new CSharpInvoke(backResolveMethod, new CSharpStatement[0]), "=", true),
                };
                var ifStatement = new CSharpIf(condition, statements);
                var resolveBody = new CSharpStatement[]{
                    ifStatement,
                    new CSharpReturn(new CSharpSymbol(fieldName))
                };
                var resolveName = GetResolveNameFront(singleton.Binding);
                var resolveMethod = new CSharpClassMethod(singleton.Binding, resolveName,
                                                new MethodParameter[0], true, resolveBody);
                members.Add(resolveMethod);
            }

            return members;
        }

        private IEnumerable<CSharpClassMember> CreateTransients(ContainerFactory builderInstance)
        {
            var members = new List<CSharpClassMember>();
            var transients = builderInstance.Registrations
                .Where(t => t.Lifetime == LifetimeScope.Transient).ToArray();

            foreach (var transient in transients)
            {
                var backResolveMethod = GetResolveName(transient.Implementation);

                var resultStatement = new CSharpInitVariable(null, "result", new CSharpInvoke(backResolveMethod, new CSharpStatement[0]));
                var resolveBody = new CSharpStatement[]{
                    resultStatement,
                    new CSharpReturn(new CSharpSymbol("result"))
                };
                var resolveName = GetResolveNameFront(transient.Binding);
                var resolveMethod = new CSharpClassMethod(transient.Binding, resolveName,
                                                new MethodParameter[0], true, resolveBody);
                members.Add(resolveMethod);
            }

            return members;
        }

        private IEnumerable<CSharpClassMember> CreateResolvers(ContainerFactory builderInstance)
        {
            var members = new List<CSharpClassMember>();

            foreach (var registration in builderInstance.Registrations)
            {
                var targetType = registration.Implementation;
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

        private IEnumerable<CSharpClassMember> CreateMainResolver(ContainerFactory builderInstance)
        {
            var members = new List<CSharpClassMember>();

            var tableType = typeof(IDictionary<Type, Func<object>>);
            var tableTypeImpl = typeof(Dictionary<Type, Func<object>>);
            var field = new CSharpField(tableType, "_ResolveTable", false);
            members.Add(field);

            var initStatements = new List<CSharpStatement>();
            var initField = new CSharpBinaryStatement(new CSharpSymbol("_ResolveTable"),
                new CSharpNewObject(tableTypeImpl, new CSharpStatement[0]), "=", true);
            initStatements.Add(initField);
            foreach (var registration in builderInstance.Registrations)
            {
                var leftInit = new CSharpSymbol($"_ResolveTable[typeof({registration.Binding.GetRightFullName()})]");
                var rightInit = new CSharpSymbol($"(Func<object>){GetResolveNameFront(registration.Binding)}");
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

            var resolveMethod = new CSharpClassMethod(typeof(object), nameof(ContainerFactory.Resolve),
                                            new MethodParameter[] { new MethodParameter(typeof(Type), "target") },
                                            true, statements, new[] { "override" });
            members.Add(resolveMethod);

            return members;
        }

        public string GetSingletonName(Type type)
        {
            return $"i_{type.GetTypeSafeName()}";
        }

        public string GetResolveNameFront(Type type)
        {
            return $"F_Resolve_{type.GetTypeSafeName()}";
        }

        public string GetResolveName(Type type)
        {
            return $"Resolve_{type.GetTypeSafeName()}";
        }

        public string GetDependencyName(Type type)
        {
            return $"dep_{type.GetTypeSafeName()}";
        }
    }
}