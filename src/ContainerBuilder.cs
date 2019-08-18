using System;
using System.Collections.Generic;

namespace Stator
{
    public abstract class ContainerBuilder
    {
        public List<ContainerRegistration> Registrations { get; private set; }

        public ContainerBuilder()
        {
            this.Registrations = new List<ContainerRegistration>();
        }

        public ContainerBuilder Add(ContainerRegistration registration)
        {
            Registrations.Add(registration);
            return this;
        }

        public ContainerBuilder AddSingleton<TBind, TImpl>()
        {
            Add(new ContainerRegistration
            {
                TypeFront = typeof(TBind),
                TypeBack = typeof(TImpl),
                Lifetime = LifetimeScope.Singleton
            });
            return this;
        }
        
        public ContainerBuilder AddSingleton<TBindAndImpl>()
        {
            Add(new ContainerRegistration
            {
                TypeFront = typeof(TBindAndImpl),
                TypeBack = typeof(TBindAndImpl),
                Lifetime = LifetimeScope.Singleton
            });
            return this;
        }

        public virtual object Resolve(Type type)
        {
            throw new NotImplementedException($"Not implemented resolver for type {this.GetType()}. Generate it!");
        }
    }
}