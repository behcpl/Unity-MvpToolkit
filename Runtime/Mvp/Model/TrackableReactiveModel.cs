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

        // Creates CancellationToken that will be cancelled when nothing is using this model anymore.
        protected CancellationToken CancelOnEnd()
        {
            _cancellation ??= new CancellationTokenSource();

            return _cancellation.Token;
        }
        
        // Automatically disposes an object when nothing is using this model anymore.
        protected void DisposeOnEnd(IDisposable disposable)
        {
            _disposeOnEnd ??= new List<IDisposable>();

            _disposeOnEnd.Add(disposable);
        }
        
        // Called when first presenter starts using this model. 
        protected virtual void OnBegin() { }

        // Called when last presenter stops using this model. 
        protected virtual void OnEnd() { }
    }
}