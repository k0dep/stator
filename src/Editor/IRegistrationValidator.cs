using System.Collections.Generic;

namespace Stator.Editor
{
    public interface IRegistrationValidator
    {
        void Validate(ContainerFactory factory, ContainerRegistration registration, IList<string> errors);
    }
}