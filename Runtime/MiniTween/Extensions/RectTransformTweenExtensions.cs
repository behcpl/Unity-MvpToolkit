using UnityEngine;

namespace Behc.MiniTween.Extensions
{
    public static class RectTransformTweenExtensions
    {
        public static ITween AnimateAnchoredPosition(this RectTransform rt, ITweenSystem system, Vector2 to, float duration)
        {
            return system.Animate(rt, SetAnchoredPosition, rt.anchoredPosition, to, duration);
        }

        public static ITween AnimateScale(this RectTransform rt, ITweenSystem system, Vector3 to, float duration)
        {
            return system.Animate(rt, SetScale, rt.localScale, to, duration);
        }

        private static void SetAnchoredPosition(object rt, Vector4 value) => ((RectTransform) rt).anchoredPosition = value;
        private static void SetScale(object rt, Vector4 value) => ((RectTransform) rt).localScale = value;
    }
}