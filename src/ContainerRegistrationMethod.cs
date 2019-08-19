using System;

namespace Stator
{
    public class ContainerRegistrationMethod : ContainerRegistration
    {
        public ContainerRegistrationMethod(Type binding, Func<object> factoryMethod, LifetimeScope lifetime)
        {
            FactoryMethod = factoryMethod;
            Binding = binding;
            Lifetime = lifetime;
        }

        public Func<object> FactoryMethod { get; set; }
    }
}