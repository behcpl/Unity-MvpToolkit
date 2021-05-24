using System;
using UnityEngine;

namespace Behc.MiniTween
{
    public interface ITweenSystem
    {
        ITween Animate(object owner, Action<object, Vector4> setter, Vector4 from, Vector4 to, float duration);
    }

    public static class RectTransformTween
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

    public static class CanvasGroupTween
    {
        public static ITween AnimateAlpha(this CanvasGroup cg, ITweenSystem system, float to, float duration)
        {
            return system.Animate(cg, SetAlpha, new Vector4(cg.alpha, 0, 0, 0), new Vector4(to, 0, 0, 0), duration);
        }

        private static void SetAlpha(object cg, Vector4 value) => ((CanvasGroup) cg).alpha = value.x;
    }
}