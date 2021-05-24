using UnityEngine;

namespace Behc.MiniTween
{
    public abstract class TweenProvider : ScriptableObject
    {
        public abstract ITweenSystem GetTweenSystem();
    }
}