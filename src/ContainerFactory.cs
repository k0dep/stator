using System;
using System.Collections.Generic;

namespace Stator
{
    public abstract class ContainerFactory : IContainerFactory
    {
        public List<ContainerRegistration> Registrations { get; private set; }

        public ContainerFactory()
        {
            Registrations = new List<ContainerRegistration>();
        }

        protected ContainerFactory Add(ContainerRegistration registration)
        {
            Registrations.Add(registration);
            return this;
        }

        protected ContainerFactory AddSingleton<TBind, TImpl>()
        {
            Add(new ContainerRegistration
            {
                TypeFront = typeof(TBind),
                TypeBack = typeof(TImpl),
                Lifetime = LifetimeScope.Singleton
            });
            return this;
        }

        protected ContainerFactory AddSingleton<TBindAndImpl>()
        {
            AddTransient<TBindAndImpl, TBindAndImpl>();
            return this;
        }

        protected ContainerFactory AddTransient<TBind, TImpl>()
        {
            Add(new ContainerRegistration
            {
                TypeFront = typeof(TBind),
                TypeBack = typeof(TImpl),
                Lifetime = LifetimeScope.Singleton
            });
            return this;
        }

        protected ContainerFactory AddTransient<TBindAndImpl>()
        {
            AddTransient<TBindAndImpl, TBindAndImpl>();
            return this;
        }

        public virtual object Resolve(Type type)
        {
            throw new NotImplementedException($"Not implemented resolver for type {this.GetType()}. Generate it!");
        }
    }
}