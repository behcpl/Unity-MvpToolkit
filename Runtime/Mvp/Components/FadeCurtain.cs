using Behc.MiniTween;
using Behc.MiniTween.Extensions;
using UnityEngine;
using UnityEngine.Assertions;

namespace Behc.Mvp.Components
{
    public class FadeCurtain : Curtain
    {
#pragma warning disable CS0649
        [SerializeField] private TweenProvider _tweenProvider;
        [SerializeField] private CanvasGroup _mainGroup;
        [SerializeField] private Canvas _mainCanvas;

        [SerializeField] private CanvasGroup _auxGroup;
        [SerializeField] private Canvas _auxCanvas;

        [SerializeField] [Range(1.0f / 255.0f, 1.0f)]
        private float _alpha = 0.5f;

        [SerializeField] private float _fadeDuration = 0.5f;
#pragma warning restore CS0649

        private ITween _tween;

        private void OnDisable()
        {
            _tween?.Kill();
            _tween = null;
        }

        public override void Setup(bool visible, int order)
        {
            _mainGroup.gameObject.SetActive(visible);
            _auxGroup.gameObject.SetActive(false);

            if (visible)
            {
                Assert.IsTrue(_mainGroup.gameObject.activeSelf);

                _mainGroup.alpha = _alpha;
                _mainCanvas.sortingOrder = order - 1;
            }
        }

        public override void Show(int order)
        {
            _tween?.Kill();

            _mainGroup.gameObject.SetActive(true);
            _auxGroup.gameObject.SetActive(false);

            Assert.IsTrue(_mainGroup.gameObject.activeSelf);

            _mainGroup.alpha = 0;
            _mainCanvas.sortingOrder = order - 1;

            _tween = _mainGroup.AnimateAlpha(_tweenProvider.GetTweenSystem(), _alpha, _fadeDuration);
            _tween.SetCompleteCallback(() => _tween = null);
        }

        public override void Switch(int newOrder, int previousOrder)
        {
            //TODO: check for same order parameters, avoid redundant animation resets 
            _tween?.Kill();

            _mainGroup.gameObject.SetActive(true);
            _auxGroup.gameObject.SetActive(true);

            Assert.IsTrue(_mainGroup.gameObject.activeSelf);
            Assert.IsTrue(_auxGroup.gameObject.activeSelf);

            _mainGroup.alpha = 0;
            _mainCanvas.sortingOrder = newOrder - 1;

            _auxGroup.alpha = _alpha;
            _auxCanvas.sortingOrder = previousOrder - 1;

            _tween = _auxGroup.AnimateAlpha(_tweenProvider.GetTweenSystem(), 0, _fadeDuration);
            _tween.SetUpdateCallback(() =>
            {
                // float s = _tweener.ElapsedPercentage(false);
                float cs = _auxGroup.alpha;
                _mainGroup.alpha = (_alpha - cs) / (1.0f - cs);
            });
            _tween.SetCompleteCallback(() =>
            {
                _auxGroup.gameObject.SetActive(false);
                _tween = null;
            });
        }

        public override void Hide()
        {
            _tween?.Kill();

            _mainGroup.gameObject.SetActive(true);
            _auxGroup.gameObject.SetActive(false);

            Assert.IsTrue(_mainGroup.gameObject.activeSelf);

            _tween = _mainGroup.AnimateAlpha(_tweenProvider.GetTweenSystem(), 0, _fadeDuration);
            _tween.SetCompleteCallback(() =>
            {
                _mainGroup.gameObject.SetActive(false);
                _tween = null;
            });
        }
    }
}