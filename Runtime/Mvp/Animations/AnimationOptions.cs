using Behc.MiniTween;
using UnityEngine;

namespace Behc.Mvp.Animations
{
    public abstract class AnimationOptions : ScriptableObject
    {
        public TweenProvider TweenProvider;
        
        public abstract IAnimator CreateAnimator(RectTransform transform, CanvasGroup canvasGroup);
    }
}