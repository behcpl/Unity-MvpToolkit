using UnityEngine;
using UnityEngine.UI;

namespace Behc.MiniTween.Extensions
{
    public static class ImageTweenExtensions
    {
        public static ITween AnimateColor(this Image img, ITweenSystem system, Color to, float duration)
        {
            return system.Animate(img, SetColor, new Vector4(img.color.r, img.color.g, img.color.b, img.color.a), new Vector4(to.r, to.g, to.b, to.a), duration);
        }

        public static ITween AnimateAlpha(this Image img, ITweenSystem system, float to, float duration)
        {
            return system.Animate(img, SetAlpha, new Vector4(0, 0, 0, img.color.a), new Vector4(0, 0, 0, to), duration);
        }

        public static ITween AnimateFillAmount(this Image img, ITweenSystem system, float to, float duration)
        {
            return system.Animate(img, SetFillAmount, new Vector4(0, 0, 0, img.fillAmount), new Vector4(0, 0, 0, to), duration);
        }

        private static void SetColor(object img, Vector4 value) => ((Image)img).color = new Color(value.x, value.y, value.z, value.w);

        private static void SetAlpha(object img, Vector4 value)
        {
            Color src = ((Image)img).color;
            src.a = value.w;
            ((Image)img).color = src;
        }

        private static void SetFillAmount(object img, Vector4 value) => ((Image)img).fillAmount = value.w;
    }
}