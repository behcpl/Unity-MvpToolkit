using System;
using UnityEngine;

namespace Behc.MiniTween
{
    public interface ITweenSystem
    {
        ITween Animate(object owner, Action<object, Vector4> setter, Vector4 from, Vector4 to, float duration);
    }
}