using System;
using UnityEngine;

namespace Behc.MiniTween
{
    public interface ITween
    {
        void Kill();
        void Complete();
        void SetCurrentTime(float time);

        void SetId(string id);
        void SetUpdateCallback(Action onUpdate);
        void SetCompleteCallback(Action onComplete);
        void SetEase(AnimationCurve curve);
    }
}