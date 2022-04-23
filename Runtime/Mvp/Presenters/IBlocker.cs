using System;

namespace Behc.Mvp.Presenters
{
    public interface IBlocker
    {
        event Action<bool, object> OnBlockingStatusChange;
    }
}