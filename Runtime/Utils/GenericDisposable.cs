using System;

namespace Behc.Utils
{
    public class GenericDisposable : IDisposable
    {
        private readonly Action _disposeAction;

        public GenericDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }
}