using UnityEngine;

namespace Behc.Mvp.Animations
{
    [CreateAssetMenu(fileName = "ScaleAnimation", menuName = "MvpToolkit/ScaleAnimation", order = 200)]
    public class ScaleAnimationOptions : AnimationOptions
    {
        public float ShowScaleStart = 0.5f;
        public AnimationCurve ShowScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float ShowAlphaStart = 1.0f;
        public float ShowDuration = 0.2f; //[s]

        public float HideScaleEnd = 1.5f;
        public AnimationCurve HideScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float HideAlphaEnd = 1.0f;
        public float HideDuration = 0.2f; //[s]

        public override IAnimator CreateAnimator(RectTransform transform, CanvasGroup canvasGroup)
        {
            return new ScaleAnimation(this, transform, canvasGroup);
        }
    }
}