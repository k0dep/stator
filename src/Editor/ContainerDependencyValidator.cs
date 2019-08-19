using System;
using System.Collections.Generic;
using System.Linq;

namespace Stator.Editor
{
    public class ContainerDependencyValidator
    {
        public IDictionary<Type, IRegistrationValidator> Validators { get; set; }

        public ContainerDependencyValidator(IDictionary<Type, IRegistrationValidator> validators)
        {
            Validators = validators;
        }
        
        public bool Validate(ContainerFactory factory, IList<string> errors)
        {
            foreach(var registration in factory.Registrations)
            {
                var validator = Validators[registration.GetType()];
                validator.Validate(factory, registration, errors);
            }

            return !errors.Any();
        }
    }
}