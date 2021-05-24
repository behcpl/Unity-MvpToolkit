using System;
using UnityEngine;

namespace Behc.Mvp.Presenter
{
    public interface IPresenterFactory : IDisposable
    {
        IPresenter CreatePresenter(RectTransform parentTransform);
        void DestroyPresenter(IPresenter presenter);
    }
}