using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Configuration
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class SceneConfigurator : MonoBehaviour, IConfigurator
    {
        [SerializeField] private SceneConfigurator[] _subConfigurators;

        [NonSerialized] private List<IDisposable> _disposables;

        void IConfigurator.Load(IDependencyResolver resolver)
        {
            MiniDiActivator.CallMethod(resolver, this, "OnLoad");
            
            if (_subConfigurators != null)
            {
                foreach (IConfigurator sc in _subConfigurators)
                {
                    sc.Load(resolver);
                }
            }
        }

        void IConfigurator.Unload(IDependencyResolver resolver)
        {
            if (_subConfigurators != null)
            {
                for (int i = _subConfigurators.Length; i > 0; i--)
                {
                    IConfigurator sc = _subConfigurators[i - 1];
                    sc.Unload(resolver);
                }
            }

            MiniDiActivator.CallMethod(resolver, this, "OnUnload");

            if (_disposables != null)
            {
                foreach (IDisposable disposable in _disposables)
                {
                    disposable.Dispose();
                }

                _disposables.Clear();
                _disposables = null;
            }
        }

        protected void DisposeOnUnload([NotNull] IDisposable disposable)
        {
            _disposables ??= new List<IDisposable>();
            _disposables.Add(disposable);
        }
    }
}