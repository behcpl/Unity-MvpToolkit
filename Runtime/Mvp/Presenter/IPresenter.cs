using System;
using UnityEngine;

namespace Behc.Mvp.Presenter
{
    public interface IPresenter
    {
        bool IsActive { get; }
        bool IsAnimating { get; }

        RectTransform RectTransform { get; }
    
        void Initialize(PresenterMap presenterMap, PresenterUpdateKernel kernel);
        void Destroy();

        void Bind(object model, IPresenter parent, bool prepareForAnimation);
        void Rebind(object model);
        void Unbind();

        void AnimateShow(float startTime, Action onFinish);
        void AnimateHide(float startTime, Action onFinish);
        void AbortAnimations();

        void Activate();
        void Deactivate();

        void ScheduledUpdate();
    }
}