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
        
        public static ITween AnimateSizeDelta(this RectTransform rt, ITweenSystem system, Vector2 to, float duration)
        {
            return system.Animate(rt, SetSizeDelta, rt.sizeDelta, to, duration);
        }
        
        public static ITween AnimateOffsetMin(this RectTransform rt, ITweenSystem system, Vector2 to, float duration)
        {
            return system.Animate(rt, SetOffsetMin, rt.offsetMin, to, duration);
        }  
        
        public static ITween AnimateOffsetMax(this RectTransform rt, ITweenSystem system, Vector2 to, float duration)
        {
            return system.Animate(rt, SetOffsetMax, rt.offsetMax, to, duration);
        }
        
        private static void SetAnchoredPosition(object rt, Vector4 value) => ((RectTransform) rt).anchoredPosition = value;
        private static void SetScale(object rt, Vector4 value) => ((RectTransform) rt).localScale = value;
        private static void SetSizeDelta(object rt, Vector4 value) => ((RectTransform) rt).sizeDelta = value;
        private static void SetOffsetMin(object rt, Vector4 value) => ((RectTransform) rt).offsetMin = value;
        private static void SetOffsetMax(object rt, Vector4 value) => ((RectTransform) rt).offsetMax = value;
    }
}