using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Navigation
{
    public class NavigationRegistry : INavigationRegistry
    {
        private readonly Dictionary<string, IFactory<object, INavigable>> _factories;

        public NavigationRegistry()
        {
            _factories = new Dictionary<string, IFactory<object, INavigable>>();
        }

        [MustUseReturnValue]
        public IDisposable Register(string name, IFactory<object, INavigable> navigableFactory)
        {
            _factories.Add(name, navigableFactory);

            return new GenericDisposable(() => _factories.Remove(name));
        }

        public INavigable Create(string name, object parameters)
        {
            if (_factories.TryGetValue(name, out IFactory<object, INavigable> factory))
            {
                return factory.Create(parameters);
            }
            
            Debug.Assert(false, $"Factory for '{name}' not found!");
            return null;
        }
    }
}