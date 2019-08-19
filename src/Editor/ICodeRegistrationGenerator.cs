using System.Collections.Generic;

namespace Stator.Editor
{
    public interface ICodeRegistrationGenerator
    {
        IEnumerable<CSharpClassMember> Generate(ContainerRegistration registration);
    }

    public class MethodCodeGenerator : ICodeRegistrationGenerator
    {
        public IEnumerable<CSharpClassMember> Generate(ContainerRegistration registration)
        {
            var methodReg = registration as ContainerRegistrationMethod;
            if (methodReg == null)
            {
                return new CSharpClassMember[0];
            }

            var members = new List<CSharpClassMember>();
            if(methodReg.Lifetime == LifetimeScope.Singleton)
            {
                members.AddRange(CreateSingletons(methodReg));
            }
            else
            {
                members.Add(CreateTransients(methodReg));
            }

            return members;
        }

        private IEnumerable<CSharpClassMember> CreateSingletons(ContainerRegistrationMethod registration)
        {
            var members = new List<CSharpClassMember>();

            var binding = registration.Binding;
            var fieldName = binding.GetSingletonFieldName();
            var field = new CSharpField(binding, fieldName, false);
            members.Add(field);

            var condition = new CSharpBinaryStatement(new CSharpSymbol(fieldName), CSharpSymbol.NULL, "==");
            var statements = new CSharpStatement[] {
                new CSharpBinaryStatement(new CSharpSymbol(fieldName), new CSharpInvoke(registration.FactoryMethod, new CSharpStatement[0]), "=", true),
            };
            var ifStatement = new CSharpIf(condition, statements);
            var resolveBody = new CSharpStatement[]{
                ifStatement,
                new CSharpReturn(new CSharpSymbol(fieldName))
            };
            var resolveName = binding.GetResolveNameBind();
            var resolveMethod = new CSharpClassMethod(binding, resolveName,
                                            new MethodParameter[0], true, resolveBody);
            members.Add(resolveMethod);

            return members;
        }

        private CSharpClassMethod CreateTransients(ContainerRegistrationMethod registration)
        {
            var resultStatement = new CSharpInitVariable(null, "result", new CSharpInvoke(registration.FactoryMethod, new CSharpStatement[0]));
            var resolveBody = new CSharpStatement[]{
                resultStatement,
                new CSharpReturn(new CSharpSymbol("result"))
            };
            var resolveName = registration.Binding.GetResolveName();
            var resolveMethod = new CSharpClassMethod(registration.Binding, resolveName,
                                            new MethodParameter[0], true, resolveBody);
            return resolveMethod;
        }
    }
}