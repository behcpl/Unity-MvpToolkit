using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Configuration
{
    public class MiniDiContainer : IDependencyResolver
    {
        private readonly Dictionary<string, object> _namedInstances;
        private readonly Dictionary<Type, object> _typedInstances;

        private readonly IDependencyResolver _parentContext;

        public MiniDiContainer(IDependencyResolver parentContext = null)
        {
            _namedInstances = new Dictionary<string, object>();
            _typedInstances = new Dictionary<Type, object>();

            _parentContext = parentContext;
        }

        public void BindInstance([NotNull] Type type, [NotNull] object instance)
        {
            _typedInstances.Add(type, instance);
        }

        public void BindInstance<T>([NotNull] T instance)
        {
            BindInstance(typeof(T), instance);
        }

        public void BindInterfaceToInstance<TInterface, TInstance>([NotNull] TInstance instance) where TInstance : TInterface
        {
            BindInstance(typeof(TInterface), instance);
        }

        public void BindNamedInstance([NotNull] string name, [NotNull] object instance)
        {
            _namedInstances.Add(name, instance);
        }

        public T Resolve<T>() where T : class
        {
            if (_typedInstances.TryGetValue(typeof(T), out object value))
            {
                T resolvedValue = value as T;
                Debug.Assert(resolvedValue != null, $"Resolved value is null or not '{typeof(T).Name}' type!");
                return resolvedValue;
            }

            T parentValue = _parentContext?.Resolve<T>();
            Debug.Assert(parentValue != null, $"Instance not found for '{typeof(T).Name}' type!");
            return parentValue;
        }

        public T Resolve<T>(string name) where T : class
        {
            if (_namedInstances.TryGetValue(name, out object value))
            {
                T resolvedValue = value as T;
                Debug.Assert(resolvedValue != null, $"Resolved '{name}' value is null or not '{typeof(T).Name}' type!");
                return resolvedValue;
            }

            T parentValue =  _parentContext?.Resolve<T>(name);
            Debug.Assert(parentValue != null, $"Instance not found for '{name}' of '{typeof(T).Name}' type!");
            return parentValue;
        }
    }
}