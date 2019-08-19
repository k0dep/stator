using System;

namespace Stator
{
    public class ContainerRegistrationMethod : ContainerRegistration
    {
        public string FactoryMethod { get; set; }

        public ContainerRegistrationMethod(Type binding, string methodName, LifetimeScope lifetime)
        {
            FactoryMethod = methodName;
            Binding = binding;
            Lifetime = lifetime;
        }
    }
}