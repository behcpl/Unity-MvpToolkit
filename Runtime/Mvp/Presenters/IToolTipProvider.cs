using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public interface IToolTipProvider
    {
        // This method will be called every frame, make sure it returns cached model
        // hit GameObject is the first one from RaycastAll results,
        // only first IPresenter will be queried about IToolTipProvider
        object GetToolTip(GameObject hit);
    }
}