using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.MiniDi
{
    public class MiniDiContainer : IDependencyResolver
    {
        private readonly Dictionary<string, object> _namedInstances = new Dictionary<string, object>();
        private readonly Dictionary<Type, object> _typedInstances = new Dictionary<Type, object>();

        private readonly IDependencyResolver _parentContext;

        public MiniDiContainer()
        {
            _parentContext = null;
        }

        public MiniDiContainer(IDependencyResolver parentContext)
        {
            _parentContext = parentContext;
        }

        public IDisposable BindInstance([NotNull] Type type, [NotNull] object instance)
        {
            _typedInstances.Add(type, instance);
            return new GenericDisposable(() => _typedInstances.Remove(type));
        }

        public IDisposable BindInstance<T>([NotNull] T instance)
        {
            return BindInstance(typeof(T), instance);
        }

        public IDisposable BindInterfaceToInstance<TInterface, TInstance>([NotNull] TInstance instance) where TInstance : TInterface
        {
            return BindInstance(typeof(TInterface), instance);
        }

        public IDisposable BindNamedInstance([NotNull] string name, [NotNull] object instance)
        {
            _namedInstances.Add(name, instance);
            return new GenericDisposable(() => _namedInstances.Remove(name));
        }

        public T Resolve<T>() where T : class
        {
            if (_typedInstances.TryGetValue(typeof(T), out object value))
            {
                return value as T;
            }

            return _parentContext?.Resolve<T>();
        }

        public T Resolve<T>(string name) where T : class
        {
            if (_namedInstances.TryGetValue(name, out object value))
            {
                return value as T;
            }

            return _parentContext?.Resolve<T>(name);
        }
    }
}