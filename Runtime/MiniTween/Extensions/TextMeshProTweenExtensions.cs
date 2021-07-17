#if BEHC_MVPTOOLKIT_TEXTMESHPRO
using TMPro;
using UnityEngine;

namespace Behc.MiniTween.Extensions
{
    public static class TextMeshProTweenExtensions
    {
        public static ITween AnimateColor(this TextMeshProUGUI text, ITweenSystem system, Color to, float duration)
        {
            return system.Animate(text, SetColor, new Vector4(text.color.r, text.color.g, text.color.b, text.color.a), new Vector4(to.r, to.g, to.b, to.a), duration);
        }

        public static ITween AnimateAlpha(this TextMeshProUGUI text, ITweenSystem system, float to, float duration)
        {
            return system.Animate(text, SetAlpha, new Vector4(0, 0, 0, text.color.a), new Vector4(0, 0, 0, to), duration);
        }

        private static void SetColor(object text, Vector4 value) => ((TextMeshProUGUI) text).color = new Color(value.x, value.y, value.z, value.w);

        private static void SetAlpha(object img, Vector4 value)
        {
            Color src = ((TextMeshProUGUI) img).color;
            src.a = value.w;
            ((TextMeshProUGUI) img).color = src;
        }
    }
}
#endif