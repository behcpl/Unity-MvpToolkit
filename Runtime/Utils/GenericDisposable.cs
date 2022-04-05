using System;
using System.Collections.Generic;

namespace Behc.Utils
{
    public class GenericDisposable : IDisposable
    {
        public static GenericDisposable Noop = new GenericDisposable(null);
        
        private readonly Action _disposeAction;

        public GenericDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public GenericDisposable(IEnumerable<IDisposable> disposables, Action disposeBefore = null, Action disposeAfter = null)
        {
            _disposeAction = () =>
            {
                disposeBefore?.Invoke();

                foreach (IDisposable disposable in disposables)
                {
                    disposable.Dispose();
                }

                disposeAfter?.Invoke();
            };
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }
}