using System;
using JetBrains.Annotations;

namespace Behc.Mvp.Models
{
    public interface IReactive
    {
        [MustUseReturnValue]
        IDisposable Subscribe(Action action);
    }
}