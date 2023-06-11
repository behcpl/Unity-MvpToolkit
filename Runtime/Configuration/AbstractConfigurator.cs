using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Behc.Configuration
{
    public abstract class AbstractConfigurator : IConfigurator
    {
        private List<IDisposable> _disposables;

        void IConfigurator.Load(IDependencyResolver resolver)
        {
            MiniDiActivator.CallMethod(resolver, this, "OnLoad");
        }

        void IConfigurator.Unload(IDependencyResolver resolver)
        {
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