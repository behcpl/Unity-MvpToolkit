using System;
using Behc.MiniTween;
using Behc.MiniTween.Extensions;
using UnityEngine;

namespace Behc.Mvp.Animations
{
    public class ScaleAnimation : IAnimator
    {
        private readonly ScaleAnimationOptions _options;
        private readonly RectTransform _transform;
        private readonly CanvasGroup _canvasGroup;

        private ITween _tweenFade;
        private ITween _tweenScale;

        public bool IsAnimating => _tweenFade != null || _tweenScale != null;

        public ScaleAnimation(ScaleAnimationOptions options, RectTransform transform, CanvasGroup canvasGroup)
        {
            _options = options;
            _transform = transform;
            _canvasGroup = canvasGroup;
        }

        public void Setup(bool prepareForAnimation)
        {
            _canvasGroup.alpha = prepareForAnimation ? _options.ShowAlphaStart : 1.0f;
            _transform.localScale = prepareForAnimation ? new Vector3(_options.ShowScaleStart, _options.ShowScaleStart, _options.ShowScaleStart) : Vector3.one;
        }

        public void AnimateShow(float startTime, Action onFinish)
        {
            if (startTime >= _options.ShowDuration || _options.ShowDuration <= 0)
            {
                onFinish?.Invoke();
                return;
            }

            ITweenSystem tweenSystem = _options.TweenProvider.GetTweenSystem();
            _tweenFade = _canvasGroup.AnimateAlpha(tweenSystem, 1, _options.ShowDuration);
            _tweenScale = _transform.AnimateScale(tweenSystem, Vector3.one, _options.ShowDuration);
            _tweenScale.SetEase(_options.ShowScaleCurve);

            _tweenScale.SetCompleteCallback(() =>
            {
                _tweenFade = null;
                _tweenScale = null;
                onFinish?.Invoke();
            });

            _tweenFade.SetCurrentTime(startTime);
            _tweenScale.SetCurrentTime(startTime);
        }

        public void AnimateHide(float startTime, Action onFinish)
        {
            if (startTime >= _options.HideDuration || _options.HideDuration <= 0)
            {
                onFinish?.Invoke();
                return;
            }

            ITweenSystem tweenSystem = _options.TweenProvider.GetTweenSystem();
            _tweenFade = _canvasGroup.AnimateAlpha(tweenSystem, _options.HideAlphaEnd, _options.HideDuration);
            _tweenScale = _transform.AnimateScale(tweenSystem, new Vector3(_options.HideScaleEnd, _options.HideScaleEnd, _options.HideScaleEnd), _options.HideDuration);
            _tweenScale.SetEase(_options.HideScaleCurve);

            _tweenScale.SetCompleteCallback(() =>
            {
                _tweenFade = null;
                _tweenScale = null;
                onFinish?.Invoke();
            });

            _tweenFade.SetCurrentTime(startTime);
            _tweenScale.SetCurrentTime(startTime);
        }

        public void AbortAnimations()
        {
            _tweenFade?.Complete();
            _tweenScale?.Complete();
        }
    }
}