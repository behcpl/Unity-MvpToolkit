using System;
using Behc.MiniTween;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;

namespace Behc.Mvp.DataSlot
{
    public class DataSlotCurtainPresenter : DataSlotPresenter
    {
#pragma warning disable CS0649
        [SerializeField] private CanvasGroup _curtain;
        [SerializeField] private DataSlotCurtainPresenterOptions _options;
        [SerializeField] private TweenProvider _tweenProvider;
#pragma warning restore CS0649

        private enum State
        {
            FINISHED,
            HIDING_OLD,
            SHOWING_CURTAIN,
            HIDING_CURTAIN,
            SHOWING_NEW,
        }

        private bool _aborted;
        private State _state;
        private ITween _tween;
        private IPresenter _animation;

        public override bool IsAnimating => _state != State.FINISHED;

        protected override void DoTransition()
        {
            if (IsAnimating)
                throw new Exception("Still animating!");

            _nextModel = _model.Data;

            IPresenter fromPresenter = _activePresenter;
            object fromModel = _activeModel;

            IPresenter toPresenter = CreatePresenter(_model.Data);
            object toModel = _model.Data;

            float showDuration = 0.2f;
            float hideDuration = 0.2f;
            AnimationCurve showCurve = null;
            AnimationCurve hideCurve = null;

            ITweenSystem tweenSystem = _tweenProvider.GetTweenSystem();

            if (_options != null)
            {
                showDuration = _options.ShowDuration;
                hideDuration = _options.HideDuration;
                showCurve = _options.ShowCurve;
                hideCurve = _options.HideCurve;
            }

            if (fromPresenter != null)
            {
                _suppressActivation = true;
                if (IsActive)
                    fromPresenter.Deactivate();

                _state = State.HIDING_OLD;
                _animation = fromPresenter;
                _animation.AnimateHide(0, HandleShowCurtain);
            }
            else
            {
                HandleShowCurtain();
            }

            void HandleShowCurtain()
            {
                _animation = null;

                if (_aborted)
                {
                    Switch();
                    Finish();
                }
                else
                {
                    _curtain.alpha = 0;
                    _curtain.gameObject.SetActive(true);

                    _state = State.SHOWING_CURTAIN;
                    _tween = _curtain.AnimateAlpha(tweenSystem, 1, showDuration);
                    if (showCurve != null)
                        _tween.SetEase(showCurve);

                    _tween.SetCompleteCallback(() =>
                    {
                        if (_aborted)
                        {
                            Switch();
                            Finish();
                        }
                        else
                        {
                            CallOnKernelUpdate(HandleHideCurtain);
                        }
                    });
                }
            }

            void Switch()
            {
                if (fromPresenter != null)
                {
                    BindingHelper.Unbind(fromModel, fromPresenter);

                    PresenterMap.DestroyPresenter(fromModel, fromPresenter);
                }

                _activeModel = toModel;
                _activePresenter = toPresenter;

                if (toPresenter != null)
                {
                    BindingHelper.Bind(toModel, toPresenter, this, !_aborted);
                }
            }

            void HandleHideCurtain()
            {
                Switch();

                _state = State.HIDING_CURTAIN;
                _tween = _curtain.AnimateAlpha(tweenSystem, 0, hideDuration);
                if (hideCurve != null)
                    _tween.SetEase(hideCurve);

                _tween.SetCompleteCallback(() =>
                {
                    if (toPresenter != null && !_aborted)
                    {
                        _state = State.SHOWING_NEW;
                        _animation = toPresenter;
                        _animation.AnimateShow(0, Finish);
                    }
                    else
                    {
                        Finish();
                    }
                });
            }

            void Finish()
            {
                _nextModel = null;
                _curtain.gameObject.SetActive(false);
                _tween = null;
                _animation = null;
                _state = State.FINISHED;

                _suppressActivation = false;
                if (IsActive)
                    toPresenter?.Activate();
            }
        }

        public override void AbortAnimations()
        {
            _aborted = true;
            _animation?.AbortAnimations();
            _tween?.Complete();
            _aborted = false;
        }
    }
}