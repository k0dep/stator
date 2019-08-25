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
            Add(new ContainerRegistrationDirect(typeof(TBind), typeof(TImpl), LifetimeScope.Singleton));
            return this;
        }

        protected ContainerFactory AddSingleton<TBindAndImpl>()
        {
            AddSingleton<TBindAndImpl, TBindAndImpl>();
            return this;
        }

        protected ContainerFactory AddSingleton<TBind>(string method)
        {
            Add(new ContainerRegistrationMethod(typeof(TBind), method, LifetimeScope.Singleton));
            return this;
        }

        protected ContainerFactory AddTransient<TBind, TImpl>()
            where TImpl : TBind
        {
            Add(new ContainerRegistrationDirect(typeof(TBind), typeof(TImpl), LifetimeScope.Transient));
            return this;
        }

        protected ContainerFactory AddTransient<TBindAndImpl>()
        {
            AddTransient<TBindAndImpl, TBindAndImpl>();
            return this;
        }

        protected ContainerFactory AddTransient<TBind>(string method)
        {
            Add(new ContainerRegistrationMethod(typeof(TBind), method, LifetimeScope.Transient));
            return this;
        }

        public virtual object Resolve(Type type)
        {
            throw new NotImplementedException($"Not implemented resolver for type {this.GetType()}. Generate it!");
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }
    }
}