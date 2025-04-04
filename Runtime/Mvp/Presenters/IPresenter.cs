using System;
using UnityEngine;

namespace Behc.Mvp.Presenters
{
    public interface IPresenter
    {
        bool IsActive { get; }
        bool IsAnimating { get; }

        RectTransform RectTransform { get; }

        // Called exactly once, when presenter is created or registered (if loaded as part of scene),
        // this is especially important for object pools, as reused presenter is not initialized multiple times
        void Initialize(IPresenterMap presenterMap, PresenterUpdateKernel kernel);

        // Called before destroying GameObjects, most likely on unregister.
        void Destroy();

        // Called every time a model is bound to presenter. Cannot be called again without calling Unbind() first.
        // 'prepareForAnimation' - if true, AnimateShow will be called somewhere in future to complete show process
        void Bind(object model, IPresenter parent, bool prepareForAnimation);

        // Called to unbind a model. No references should be held at this point.
        void Unbind();

        // Called only if parent presenter supports animations and 'prepareForAnimation' was true during Bind()
        // Implementation *must* call onFinish when done (even if no animation was started).
        void AnimateShow(float startTime, Action onFinish);

        // Called only if parent presenter supports animations.
        // Implementation *must* call onFinish when done (even if no animation was started).
        void AnimateHide(float startTime, Action onFinish);

        // Called if parent presenter decides to interrupt transition and IsAnimating is true.
        // Implementation must finish any pending animations and call adequate onFinish before exiting this method.
        void AbortAnimations();

        // Called when presenter can process user input, after all animations/transitions are done, and there is no blocking layers active.
        void Activate();

        // Called before hide animations or Unbind(), or in case of activating some blocking layer.
        void Deactivate();

        // Called on presenter request, during PresenterUpdateKernel's Update.
        // Request this update using PresenterUpdateKernel.RequestUpdate(this)
        // Important: Some methods (like Bind/Unbind) can only be called during this update
        void ScheduledUpdate();

        // Called only from outside on demand
        // Steers the policy for what happens to the gameObject on unbind
        // Important: This is a Flags enum, so you can store multiple policies here
        void SetUnbindPolicies(UnbindPolicies unbindPolicies);
    }
}