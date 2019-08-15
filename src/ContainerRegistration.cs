using System;

namespace Stator
{
    public class ContainerRegistration
    {
        public Type TypeFront { get; set; }
        public Type TypeBack { get; set; }
        public LifetimeScope Lifetime { get; set; }
    }
}