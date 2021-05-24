using System;
using Behc.MiniTween;
using UnityEngine;

namespace Behc.Mvp.Animations
{
    public class FadeAnimation : IAnimator
    {
        private readonly FadeAnimationOptions _options;
        private readonly CanvasGroup _canvasGroup;
        
        private ITween _tween;

        public bool IsAnimating => _tween != null;

        public FadeAnimation(FadeAnimationOptions options, CanvasGroup canvasGroup)
        {
            _options = options;
            _canvasGroup = canvasGroup;
        }
        
        public void Setup(bool prepareForAnimation)
        {
            _canvasGroup.alpha = prepareForAnimation ? 0 : 1;
        }

        public void AnimateShow(float startTime, Action onFinish)
        {
            if (startTime >= _options.ShowDuration || _options.ShowDuration <= 0)
            {
                onFinish?.Invoke();
                return;
            }
            
            _tween = _canvasGroup.AnimateAlpha(_options.TweenProvider.GetTweenSystem(), 1, _options.ShowDuration);
            _tween.SetId($"AnimateShow-{_canvasGroup.gameObject.name}");
            _tween.SetCompleteCallback(() =>
            {
                _tween = null;
                onFinish?.Invoke();
            });
            
            _tween.SetCurrentTime(startTime);
        }

        public void AnimateHide(float startTime, Action onFinish)
        {
            if (startTime >= _options.HideDuration || _options.HideDuration <= 0)
            {
                onFinish?.Invoke();
                return;
            }
            
            _tween = _canvasGroup.AnimateAlpha(_options.TweenProvider.GetTweenSystem(), 0, _options.HideDuration);
            _tween.SetId($"AnimateHide-{_canvasGroup.gameObject.name}");
            _tween.SetCompleteCallback(() =>
            {
                _tween = null;
                onFinish?.Invoke();
            });
   
            _tween.SetCurrentTime(startTime);
        }

        public void AbortAnimations()
        {
            _tween?.Complete();
        }
    }
}