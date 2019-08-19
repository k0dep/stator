using System;

namespace Stator
{
    public class ContainerRegistrationDirect : ContainerRegistration
    {
        public Type Implementation { get; set; }

        public ContainerRegistrationDirect(Type binding, Type implementation, LifetimeScope lifetime)
        {
            Binding = binding;
            Implementation = implementation;
            Lifetime = lifetime;
        }
    }
}