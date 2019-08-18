using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class ContainerDependencyValidator
    {
        public bool Validate(ContainerFactory factory, IList<string> errors)
        {
            foreach(var registration in factory.Registrations)
            {
                if(!registration.Binding.IsAssignableFrom(registration.Implementation))
                {
                    errors.Add($"Registration resolving `{registration.Binding.GetRightFullName()}` "
                        + $"not assingable from `{registration.Implementation.GetRightFullName()}`");
                    continue;
                }

                var ctor = registration.Implementation
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
                            + $"for implementation `{registration.Implementation.GetRightFullName()}` "
                            + $"and resolving type `{registration.Binding.GetRightFullName()}`");
                    }
                }
            }

            return !errors.Any();
        }
    }
}