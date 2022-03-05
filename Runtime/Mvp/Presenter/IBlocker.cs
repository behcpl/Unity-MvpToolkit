using System;

namespace Behc.Mvp.Presenter
{
    public interface IBlocker
    {
        event Action<bool, object> OnBlockingStatusChange;
    }
}