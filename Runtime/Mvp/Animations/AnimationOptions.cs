using Behc.MiniTween;
using Behc.Utils;
using UnityEngine;

namespace Behc.Mvp.Animations
{
    public abstract class AnimationOptions : ScriptableObject
    {
        public AbstractProvider<ITweenSystem> TweenProvider;
        
        public abstract IAnimator CreateAnimator(RectTransform transform, CanvasGroup canvasGroup);
    }
}