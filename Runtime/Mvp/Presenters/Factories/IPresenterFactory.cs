using System;
using UnityEngine;

namespace Behc.Mvp.Presenters.Factories
{
    public interface IPresenterFactory : IDisposable
    {
        IPresenter CreatePresenter(RectTransform parentTransform);
        void DestroyPresenter(IPresenter presenter);
    }
}