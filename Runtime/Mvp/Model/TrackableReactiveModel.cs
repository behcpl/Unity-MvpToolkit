using System;
using System.Collections.Generic;
using System.Threading;

namespace Behc.Mvp.Model
{
    public class TrackableReactiveModel : ReactiveModel, ITrackable
    {
        private int _refCount;
        private List<IDisposable> _disposeOnEnd;
        private CancellationTokenSource _cancellation;

        public void Acquire()
        {
            if (_refCount == 0)
            {
                OnBegin();
            }

            _refCount++;
        }

        public void Release()
        {
            _refCount--;

            if (_refCount == 0)
            {
                if (_cancellation != null)
                {
                    _cancellation.Cancel();
                    _cancellation.Dispose();
                    _cancellation = null;
                }
                
                OnEnd();

                if (_disposeOnEnd != null)
                {
                    foreach (IDisposable disposable in _disposeOnEnd)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        protected CancellationToken CancelOnEnd()
        {
            _cancellation ??= new CancellationTokenSource();

            return _cancellation.Token;
        }
        
        protected void DisposeOnEnd(IDisposable disposable)
        {
            _disposeOnEnd ??= new List<IDisposable>();

            _disposeOnEnd.Add(disposable);
        }
        
        protected virtual void OnBegin() { }

        protected virtual void OnEnd() { }
    }
}