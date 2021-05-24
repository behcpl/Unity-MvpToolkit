using System;
using Behc.Utils;

namespace Behc.Mvp.Model
{
    public class ReactiveModel : IReactive
    {
        private event Action _onChange;
   
        public IDisposable Subscribe(Action action)
        {
            _onChange += action;

            return new GenericDisposable(() => _onChange -= action);
        }

        public void NotifyChanges()
        {
            _onChange?.Invoke();
        }
    }
}