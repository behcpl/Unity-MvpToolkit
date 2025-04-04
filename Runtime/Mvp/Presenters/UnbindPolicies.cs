using System;

namespace Behc.Mvp.Presenters
{
    [Flags]
    public enum UnbindPolicies
    {
        None = 0,
        DeactivateGameObject = 1,
    }
}