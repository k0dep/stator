using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class DirectRegistrationValidator : IRegistrationValidator
    {
        public void Validate(ContainerFactory factory, ContainerRegistration registration, IList<string> errors)
        {
            var direct = registration as ContainerRegistrationDirect;
            if(direct == null)
            {
                return;
            }

            if(!registration.Binding.IsAssignableFrom(direct.Implementation))
            {
                errors.Add($"Registration resolving `{direct.Binding.GetRightFullName()}` "
                    + $"not assingable from `{direct.Implementation.GetRightFullName()}`");
                return;
            }

            var ctor = direct.Implementation
                .GetConstructors()
                .OrderBy(c => c.GetParameters()
                    .Count())
                .First();
            
            var dependencies = ctor.GetParameters()
                .Select(t => t.ParameterType);
            
            foreach(var dependency in dependencies)
            {
                var isRegistred = factory.Registrations
                    .Any(t => t.Binding == dependency);
                
                if (!isRegistred)
                {
                    errors.Add($"Missing dependency `{dependency.GetRightFullName()}` " 
                        + $"for implementation `{direct.Implementation.GetRightFullName()}` "
                        + $"and resolving type `{direct.Binding.GetRightFullName()}`");
                }
            }
        }
    }
}