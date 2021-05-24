using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.MiniDi
{
    public class MiniDiContainer
    {
        private readonly Dictionary<string, object> _namedInstances = new Dictionary<string, object>();
        private readonly Dictionary<Type, object> _typedInstances = new Dictionary<Type, object>();

        public IDisposable BindInstance(Type type, object instance)
        {
            _typedInstances.Add(type, instance);
            return new GenericDisposable(() => _typedInstances.Remove(type));
        }

        public IDisposable BindInstance<T>(T instance)
        {
            return BindInstance(typeof(T), instance);
        }

        public IDisposable BindInterfaceToInstance<TInterface, TInstance>(TInstance instance)
        {
            return BindInstance(typeof(TInterface), instance);
        }

        public IDisposable BindNamedInstance(string name, object instance)
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

            return null;
        }

        public T Resolve<T>(string name) where T : class
        {
            if (_namedInstances.TryGetValue(name, out object value))
            {
                return value as T;
            }

            return null;
        }
    }
}