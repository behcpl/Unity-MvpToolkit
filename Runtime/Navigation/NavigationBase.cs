using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public abstract class NavigableBase : INavigable
    {
        private List<IDisposable> _stopDisposables;
        private List<IDisposable> _disposables;
        private CancellationTokenSource _cancellation;

        protected virtual void OnStart()
        {
        }

        protected virtual void OnRestart(object parameters)
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void OnDispose(Action onComplete)
        {
            onComplete?.Invoke();
        }

        protected void DisposeOnStop([NotNull] IDisposable disposable)
        {
            _stopDisposables ??= new List<IDisposable>();
            _stopDisposables.Add(disposable);
        }

        protected void AutoDispose([NotNull] IDisposable disposable)
        {
            _disposables ??= new List<IDisposable>();
            _disposables.Add(disposable);
        }

        protected CancellationToken CancelOnStop()
        {
            _cancellation ??= new CancellationTokenSource();

            return _cancellation.Token;
        }

        void INavigable.Start()
        {
            OnStart();
        }

        void INavigable.Restart(object parameters)
        {
            OnRestart(parameters);
        }

        void INavigable.Stop()
        {
            if (_cancellation != null)
            {
                _cancellation.Cancel();
                _cancellation.Dispose();
                _cancellation = null;
            }

            OnStop();

            if (_stopDisposables != null)
            {
                foreach (IDisposable disposable in _stopDisposables)
                    disposable.Dispose();
                _stopDisposables.Clear();
            }
        }

        void INavigable.LongDispose(Action onComplete)
        {
            OnDispose(Complete);

            void Complete()
            {
                if (_disposables != null)
                {
                    foreach (IDisposable disposable in _disposables)
                        disposable.Dispose();
                    _disposables.Clear();
                }

                onComplete?.Invoke();
            }
        }
    }
}