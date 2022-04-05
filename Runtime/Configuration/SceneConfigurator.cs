using System;
using System.Collections.Generic;
using Behc.MiniDi;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Configuration
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class SceneConfigurator : MonoBehaviour, IConfigurator
    {
        [SerializeField] private SceneConfigurator[] _subConfigurators;

        [NonSerialized] private List<IDisposable> _disposables;

        protected virtual void OnLoad(IDependencyResolver resolver) { }
        protected virtual void OnUnload(IDependencyResolver resolver) { }

        void IConfigurator.Load(IDependencyResolver resolver)
        {
            OnLoad(resolver);

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

            OnUnload(resolver);

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