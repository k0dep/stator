using System;

namespace Stator
{
    public class ContainerRegistration
    {
        public Type Binding { get; set; }
        public Type Implementation { get; set; }
        public LifetimeScope Lifetime { get; set; }
    }
}