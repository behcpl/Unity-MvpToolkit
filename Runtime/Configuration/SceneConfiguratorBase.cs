using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behc.Configuration
{
    public abstract class SceneConfiguratorBase<TContext> : MonoBehaviour, ISceneConfigurator<TContext>
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public abstract void Load(TContext ctx);

        public virtual void Unload(TContext ctx)
        {
            _disposables.ForEach(d => d.Dispose());
            _disposables.Clear();
        }

        protected void DisposeOnUnload(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }
}