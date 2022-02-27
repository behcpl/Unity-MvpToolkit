using System;

namespace Behc.Mvp.Model
{
    public interface IReactive
    {
        IDisposable Subscribe(Action action);
    }
}