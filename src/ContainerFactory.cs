using System;
using System.Collections.Generic;

namespace Stator
{
    public abstract class ContainerFactory : IContainerFactory
    {
        public IEnumerable<ContainerRegistration> Registrations => _registrations;
        private List<ContainerRegistration> _registrations;

        public ContainerFactory()
        {
            _registrations = new List<ContainerRegistration>();
        }

        protected ContainerFactory Add(ContainerRegistration registration)
        {
            _registrations.Add(registration);
            return this;
        }

        protected ContainerFactory AddSingleton<TBind, TImpl>()
            where TImpl : TBind
        {
            Add(new ContainerRegistration
            {
                Binding = typeof(TBind),
                Implementation = typeof(TImpl),
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
            where TImpl : TBind
        {
            Add(new ContainerRegistration
            {
                Binding = typeof(TBind),
                Implementation = typeof(TImpl),
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