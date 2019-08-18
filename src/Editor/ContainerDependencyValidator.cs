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
                var ctor = registration.TypeBack
                    .GetConstructors()
                    .OrderBy(c => c.GetParameters()
                        .Count())
                    .First();
                
                var dependencies = ctor.GetParameters()
                    .Select(t => t.ParameterType);
                
                foreach(var dependency in dependencies)
                {
                    var isRegistred = factory.Registrations
                        .Any(t => t.TypeFront == dependency);
                    
                    if (!isRegistred)
                    {
                        errors.Add($"Missing dependency `{dependency.GetRightFullName()}` " 
                            + $"for implementation `{registration.TypeBack.GetRightFullName()}` "
                            + $"and resolving type `{registration.TypeFront.GetRightFullName()}`");
                    }
                }
            }

            return !errors.Any();
        }
    }
}