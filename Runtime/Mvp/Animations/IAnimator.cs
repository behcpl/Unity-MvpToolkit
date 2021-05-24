using System;

namespace Behc.Mvp.Animations
{
    public interface IAnimator
    {
        bool IsAnimating { get; }

        void Setup(bool prepareForAnimation);
        void AnimateShow(float startTime, Action onFinish);
        void AnimateHide(float startTime, Action onFinish);
        void AbortAnimations();
    }

    public static class AnimatorExtensions
    {
        public static void AnimateShowOrFinish(this IAnimator animator, float startTime, Action onFinish)
        {
            if(animator != null)
                animator.AnimateShow(startTime, onFinish);
            else
                onFinish?.Invoke();
        } 
        
        public static void AnimateHideOrFinish(this IAnimator animator, float startTime, Action onFinish)
        {
            if(animator != null)
                animator.AnimateHide(startTime, onFinish);
            else
                onFinish?.Invoke();
        }
    }
}