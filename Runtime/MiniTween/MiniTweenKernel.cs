using System;
using System.Collections.Generic;
using UnityEngine;

namespace Behc.MiniTween
{
    public class MiniTweenKernel : MonoBehaviour, ITweenSystem
    {
        private const int _INIT_SIZE = 16;
        private MiniTweenProvider _options;

        private class Tween : ITween
        {
            public Vector4 ValueFrom;
            public Vector4 ValueTo;

            public float Duration;
            public float Time;
            public bool Completed;

            public string Id;
            public object Owner;
            public Action OnUpdate;
            public Action OnComplete;
            public Action<object, Vector4> Setter;
            
            public AnimationCurve Curve;
            public Func<Tween, float, float> EaseFunc = EaseNone;

            public void Kill()
            {
                Completed = true;
            }

            public void Complete()
            {
                if (Completed)
                    return;
                
                Setter(Owner, ValueTo);
                OnUpdate?.Invoke();

                OnComplete?.Invoke();
                Completed = true;
            }

            public void SetCurrentTime(float time)
            {
                if (Completed)
                    return;

                Time = Mathf.Clamp(time, 0.0f, Duration);
                if (time >= Duration)
                    Complete();
            }

            public void SetId(string id)
            {
                Id = id;
            }

            public void SetUpdateCallback(Action onUpdate)
            {
                OnUpdate = onUpdate;
            }

            public void SetCompleteCallback(Action onComplete)
            {
                OnComplete = onComplete;
            }

            public void SetEase(AnimationCurve curve)
            {
                Curve = curve;
                EaseFunc = EaseCurve;
            }

            public static float EaseNone(Tween tween, float t) => t;
            public static float EaseCurve(Tween tween, float t) => tween.Curve.Evaluate(t);
        }

        private readonly List<Tween> _activeTweens = new List<Tween>(_INIT_SIZE);
        private readonly List<Tween> _newTweens = new List<Tween>(_INIT_SIZE);
        private readonly List<Tween> _unusedPool = new List<Tween>(_INIT_SIZE);

        public void Initialize(MiniTweenProvider options)
        {
            _options = options;

            for (int i = 0; i < _INIT_SIZE; i++)
            {
                _unusedPool.Add(AllocateNew());
            }
        }

        public ITween Animate(object owner, Action<object, Vector4> setter, Vector4 from, Vector4 to, float duration)
        {
            Tween tween = Allocate();
            tween.ValueFrom = from;
            tween.ValueTo = to;
            tween.Time = 0;
            tween.Duration = duration;
            tween.Completed = false;

            tween.Owner = owner;
            tween.Setter = setter;

            _newTweens.Add(tween);

            return tween;
        }

        private void Update()
        {
            float dt = _options.UnscaledTime ? Time.unscaledDeltaTime : Time.smoothDeltaTime;

            foreach (Tween newTween in _newTweens)
            {
                if(newTween.Completed)
                    Deallocate(newTween);
                else
                    _activeTweens.Add(newTween);
            }
            _newTweens.Clear();
            
            foreach (Tween tween in _activeTweens)
            {
                if (tween.Completed)
                {
                    Deallocate(tween);
                    continue;
                }

                tween.Time += dt;
                if (tween.Time >= tween.Duration)
                {
                    tween.Setter(tween.Owner, tween.ValueTo);
                    tween.OnUpdate?.Invoke();
                    tween.Complete();
                    Deallocate(tween);
                }
                else
                {
                    float s = tween.EaseFunc(tween, Mathf.Clamp01(tween.Time / tween.Duration));
                    Vector4 value = Vector4.LerpUnclamped(tween.ValueFrom, tween.ValueTo, s);
                    tween.Setter(tween.Owner, value);
                    tween.OnUpdate?.Invoke();
                }
            }

            _activeTweens.RemoveAll(t => t.Completed);
        }

        private Tween Allocate()
        {
            if (_unusedPool.Count > 0)
            {
                Tween item = _unusedPool[_unusedPool.Count - 1];
                item.Completed = false;
                _unusedPool.RemoveAt(_unusedPool.Count - 1);
                return item;
            }

            return AllocateNew();
        }

        private void Deallocate(Tween item)
        {
            item.Id = null;
            item.OnComplete = null;
            item.OnUpdate = null;
            item.Owner = null;
            item.Setter = null;
            item.Curve = null;
            item.EaseFunc = Tween.EaseNone;
            item.Completed = true;
            _unusedPool.Add(item);
        }

        private Tween AllocateNew()
        {
            return new Tween { Completed = true };
        }
    }
}