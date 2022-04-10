using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Behc.Configuration
{
    public abstract class AbstractConfigurator : IConfigurator
    {
        private List<IDisposable> _disposables;

        protected abstract void OnLoad(IDependencyResolver context);
        protected virtual void OnUnload(IDependencyResolver context) { }

        void IConfigurator.Load(IDependencyResolver context)
        {
            OnLoad(context);
        }

        void IConfigurator.Unload(IDependencyResolver context)
        {
            OnUnload(context);

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