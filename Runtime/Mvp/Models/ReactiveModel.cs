using System;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.Mvp.Models
{
    public class ReactiveModel : IReactive
    {
        private event Action _onChange;
   
        [MustUseReturnValue]
        public IDisposable Subscribe(Action action)
        {
            _onChange += action;

            return new GenericDisposable(() => _onChange -= action);
        }

        protected void NotifyChanges()
        {
            _onChange?.Invoke();
        }
    }
}