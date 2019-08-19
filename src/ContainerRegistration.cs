using System;

namespace Stator
{
    public abstract class ContainerRegistration
    {
        public Type Binding { get; set; }
        public LifetimeScope Lifetime { get; set; }
    }
}