using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stator.Editor
{
    public class StatorCodeGenerator
    {
        public ContainerDependencyValidator Validator { get; set; }
        public IDictionary<Type, ICodeRegistrationGenerator> RegistrationGenerators { get; set; }

        public StatorCodeGenerator(ContainerDependencyValidator validator,
            IDictionary<Type, ICodeRegistrationGenerator> registrationGenerators)
        {
            Validator = validator;
            RegistrationGenerators = registrationGenerators;
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
                throw new ArgumentException(
                    $"Cant create factory for factorytype {factory}. Missing default constructor.");
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

            foreach (var registration in instance.Registrations)
            {
                var generator = RegistrationGenerators[registration.GetType()];
                var result = generator.Generate(registration);
                members.AddRange(result);
            }

            members.AddRange(CreateMainResolver(instance));
            members = members.OrderByDescending(m => m.GetType().ToString()).ToList();

            var className = factoryType.Name;
            var type = new CSharpClass(className, members, true);
            var ns = new CSharpNamespace(factoryType.Namespace, new CSharpClass[] {type});
            var file = new CSharpFile(new string[] {"System"}, new CSharpNamespace[] {ns});

            file.Generate(codeBuilder);

            return codeBuilder.ToString();
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
                var rightInit = new CSharpSymbol($"(Func<object>){registration.Binding.GetResolveNameBind()}");
                var resolverRow = new CSharpBinaryStatement(leftInit, rightInit, "=", true);
                initStatements.Add(resolverRow);
            }

            var ifStatement =
                new CSharpIf(new CSharpBinaryStatement(new CSharpSymbol("_ResolveTable"), CSharpSymbol.NULL, "=="),
                    initStatements);

            var throwStatement = new CSharpThrowStatement(new CSharpNewObject(typeof(StatorUnresolvedException),
                new[] {new CSharpSymbol("target")}));
            var resultVariable =
                new CSharpInitVariable(null, "result", new CSharpInvoke("resolver", new CSharpStatement[0]));
            var checkStatement =
                new CSharpIf(
                    new CSharpInvoke("_ResolveTable.TryGetValue",
                        new CSharpStatement[] {new CSharpSymbol("target"), new CSharpSymbol("out var resolver")}),
                    new CSharpStatement[] {resultVariable, new CSharpReturn(new CSharpSymbol("result"))});

            var statements = new List<CSharpStatement>();
            statements.Add(ifStatement);
            statements.Add(checkStatement);
            statements.Add(throwStatement);

            var resolveMethod = new CSharpClassMethod(typeof(object), nameof(ContainerFactory.Resolve),
                new MethodParameter[] {new MethodParameter(typeof(Type), "target")},
                true, statements, new[] {"override"});
            members.Add(resolveMethod);

            return members;
        }
    }
}