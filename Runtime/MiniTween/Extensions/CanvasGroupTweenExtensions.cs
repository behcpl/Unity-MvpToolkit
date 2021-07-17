using UnityEngine;

namespace Behc.MiniTween.Extensions
{
    public static class CanvasGroupTweenExtensions
    {
        public static ITween AnimateAlpha(this CanvasGroup cg, ITweenSystem system, float to, float duration)
        {
            return system.Animate(cg, SetAlpha, new Vector4(0, 0, 0, cg.alpha), new Vector4(0, 0, 0, to), duration);
        }

        private static void SetAlpha(object cg, Vector4 value) => ((CanvasGroup) cg).alpha = value.w;
    }
}