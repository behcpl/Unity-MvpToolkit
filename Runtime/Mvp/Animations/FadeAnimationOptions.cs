using UnityEngine;

namespace Behc.Mvp.Animations
{
    [CreateAssetMenu(fileName = "FadeAnimation", menuName = "MvpToolkit/FadeAnimation", order = 200)]
    public class FadeAnimationOptions : AnimationOptions
    {
        public float ShowDuration = 0.2f;
        public float HideDuration = 0.2f;

        public override IAnimator CreateAnimator(RectTransform transform, CanvasGroup canvasGroup)
        {
            return new FadeAnimation(this, canvasGroup);
        }
    }
}