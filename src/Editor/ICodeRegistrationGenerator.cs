using System.Collections.Generic;

namespace Stator.Editor
{
    public interface ICodeRegistrationGenerator
    {
        IEnumerable<CSharpClassMember> Generate(ContainerRegistration registration);
    }
}