using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Assertions;

namespace Behc.Configuration
{
    public class MiniDiContainer : IDependencyResolver
    {
        private readonly Dictionary<Type, InstanceDescriptor> _descriptors;
        private readonly IDependencyResolver _parentContext;

        private class InstanceDescriptor
        {
            public object UnnamedInstance;
            public List<KeyValuePair<string, object>> NamedInstances;
        }

        public MiniDiContainer(IDependencyResolver parentContext = null)
        {
            _descriptors = new Dictionary<Type, InstanceDescriptor>();
            _parentContext = parentContext;
        }

        public void BindInstance([NotNull] Type type, [NotNull] object instance)
        {
            Assert.IsNotNull(type);
            Assert.IsNotNull(instance);

            if (!_descriptors.TryGetValue(type, out InstanceDescriptor descriptor))
            {
                descriptor = new InstanceDescriptor();
                _descriptors.Add(type, descriptor);
            }

            Assert.IsNull(descriptor.UnnamedInstance);
            descriptor.UnnamedInstance = instance;
        }

        public void BindInstance([NotNull] Type type, [NotNull] string name, [NotNull] object instance)
        {
            Assert.IsNotNull(type);
            Assert.IsNotNull(name);
            Assert.IsNotNull(instance);
            Assert.IsFalse(string.IsNullOrEmpty(name));

            if (!_descriptors.TryGetValue(type, out InstanceDescriptor descriptor))
            {
                descriptor = new InstanceDescriptor();
                _descriptors.Add(type, descriptor);
            }

            string normalizedName = Normalize(name);
            descriptor.NamedInstances ??= new List<KeyValuePair<string, object>>();

            foreach (var kv in descriptor.NamedInstances)
            {
                Assert.AreNotEqual(normalizedName, kv.Key);
            }

            descriptor.NamedInstances.Add(new KeyValuePair<string, object>(normalizedName, instance));
        }

        public void BindInstance<T>([NotNull] T instance)
        {
            BindInstance(typeof(T), instance);
        }

        public void BindNamedInstance<T>([NotNull] string name, [NotNull] T instance)
        {
            BindInstance(typeof(T), name, instance);
        }

        public object Resolve(Type type, string name, bool allowUnnamed)
        {
            if (type == typeof(IDependencyResolver))
                return this;
            
            InstanceDescriptor descriptor;
            if (string.IsNullOrEmpty(name))
            {
                if (_descriptors.TryGetValue(type, out descriptor) && descriptor.UnnamedInstance != null)
                    return descriptor.UnnamedInstance;

                return _parentContext?.Resolve(type, null, true);
            }

            string normalizedName = Normalize(name);

            if (_descriptors.TryGetValue(type, out descriptor) && descriptor.NamedInstances != null)
            {
                foreach (var kv in descriptor.NamedInstances)
                {
                    if (kv.Key == normalizedName)
                        return kv.Value;
                }
            }

            object value = _parentContext?.Resolve(type, normalizedName, false);
            if (value != null)
                return value;

            if (!allowUnnamed)
                return null;

            if (descriptor?.UnnamedInstance != null)
                return descriptor.UnnamedInstance;

            return _parentContext?.Resolve(type, null, true);
        }

        private static string Normalize(string name) => name.ToLowerInvariant().Trim('_');
    }
}