using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public abstract class NavigationBase : INavigable
    {
        public abstract void StartUp(object parameters, out object context);

        public abstract void Resume(object parameters, object context);

        public virtual void UpdateParameters(object parameters, object context) { }

        public virtual void Pause(object context) { }

        public virtual void TearDown(object context, Action onComplete)
        {
            onComplete?.Invoke();
        }
    }

    public abstract class NavigationBase2 : INavigable
    {
        private List<IDisposable> _pauseDisposables;
        private List<IDisposable> _tearDownDisposables;

        protected abstract object CreateContext(object parameters);

        protected virtual void OnResume(object parameters, object context) { }
        
        protected virtual void OnUpdateParameters(object parameters, object context) { }
        
        protected virtual void OnPause(object context) { }

        protected virtual void OnTearDown(object context, Action onComplete)
        {
            onComplete?.Invoke();
        }

        protected void DisposeOnPause([NotNull] IDisposable disposable)
        {
            _pauseDisposables ??= new List<IDisposable>();
            _pauseDisposables.Add(disposable);
        }

        protected void DisposeOnTearDown([NotNull] IDisposable disposable)
        {
            _tearDownDisposables ??= new List<IDisposable>();
            _tearDownDisposables.Add(disposable);
        }

        void INavigable.StartUp(object parameters, out object context)
        {
            context = CreateContext(parameters);
        }

        void INavigable.Resume(object parameters, object context)
        {
            OnResume(parameters, context);
        }
        
        void INavigable.UpdateParameters(object parameters, object context)
        {
            OnUpdateParameters(parameters, context);
        }
        
        void INavigable.Pause(object context)
        {
            OnPause(context);

            if (_pauseDisposables != null)
            {
                foreach (IDisposable disposable in _pauseDisposables)
                    disposable.Dispose();
                _pauseDisposables.Clear();
            }
        }

        void INavigable.TearDown(object context, Action onComplete)
        {
            OnTearDown(context, Complete);

            void Complete()
            {
                if (_tearDownDisposables != null)
                {
                    foreach (IDisposable disposable in _tearDownDisposables)
                        disposable.Dispose();
                    _tearDownDisposables.Clear();
                }

                onComplete?.Invoke();
            }
        }
    }
}