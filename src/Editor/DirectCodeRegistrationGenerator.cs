using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class DirectCodeRegistrationGenerator : ICodeRegistrationGenerator
    {
        public IEnumerable<CSharpClassMember> Generate(ContainerRegistration registration)
        {
            var directReg = registration as ContainerRegistrationDirect;
            if (directReg == null)
            {
                return new CSharpClassMember[0];
            }

            var members = new List<CSharpClassMember>();
            members.Add(CreateResolvers(directReg));
            if(directReg.Lifetime == LifetimeScope.Singleton)
            {
                members.AddRange(CreateSingletons(directReg));
            }
            else
            {
                members.Add(CreateTransients(directReg));
            }

            return members;
        }

        private IEnumerable<CSharpClassMember> CreateSingletons(ContainerRegistrationDirect registration)
        {
            var members = new List<CSharpClassMember>();

            var binding = registration.Binding;
            var fieldName = binding.GetSingletonFieldName();
            var field = new CSharpField(binding, fieldName, false);
            members.Add(field);

            var backResolveMethod = registration.Implementation.GetResolveName();
            var condition = new CSharpBinaryStatement(new CSharpSymbol(fieldName), CSharpSymbol.NULL, "==");
            var statements = new CSharpStatement[] {
                new CSharpBinaryStatement(new CSharpSymbol(fieldName), new CSharpInvoke(backResolveMethod, new CSharpStatement[0]), "=", true),
            };
            var ifStatement = new CSharpIf(condition, statements);
            var resolveBody = new CSharpStatement[]{
                ifStatement,
                new CSharpReturn(new CSharpSymbol(fieldName))
            };
            var resolveName = binding.GetResolveNameBind();
            var resolveMethod = new CSharpClassMethod(binding, resolveName,
                                            new MethodParameter[0], false, resolveBody);
            members.Add(resolveMethod);

            return members;
        }

        private CSharpClassMethod CreateTransients(ContainerRegistrationDirect registration)
        {
            var backResolveMethod = registration.Implementation.GetResolveName();

            var resultStatement = new CSharpInitVariable(null, "result", new CSharpInvoke(backResolveMethod, new CSharpStatement[0]));
            var resolveBody = new CSharpStatement[]{
                resultStatement,
                new CSharpReturn(new CSharpSymbol("result"))
            };
            var resolveName = registration.Binding.GetResolveName();
            var resolveMethod = new CSharpClassMethod(registration.Binding, resolveName,
                                            new MethodParameter[0], false, resolveBody);
            return resolveMethod;
        }

        private CSharpClassMethod CreateResolvers(ContainerRegistrationDirect registration)
        {
            var targetType = registration.Implementation;
            var ctor = targetType.GetConstructors()
                .OrderBy(c => c.GetParameters().Count())
                .First();

            var statements = new List<CSharpStatement>();
            foreach (var parameter in ctor.GetParameters())
            {
                var paramType = parameter.ParameterType;
                var resolveInvoke = new CSharpInvoke(paramType.GetResolveNameBind(), new CSharpStatement[0]);
                var variableDependency = new CSharpInitVariable(null, paramType.GetDependencyName(), resolveInvoke);
                statements.Add(variableDependency);
            }

            var @params = ctor.GetParameters()
                .Select(t => t.ParameterType)
                .Select(t => t.GetDependencyName())
                .Select(p => new CSharpSymbol(p));

            var resultVariable = new CSharpInitVariable(null, "result", new CSharpNewObject(targetType, @params));
            statements.Add(resultVariable);
            statements.Add(new CSharpReturn(new CSharpSymbol("result")));

            var resolveName = targetType.GetResolveName();
            var resolveMethod = new CSharpClassMethod(targetType, resolveName,
                                            new MethodParameter[0], false, statements);

            return resolveMethod;
        }
    }
}