using System;
using Behc.MiniTween;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
// using DG.Tweening;
using UnityEngine;

namespace Behc.Mvp.DataSlot
{
    public class DataSlotSlidePresenter : DataSlotPresenter
    {
        //variants:
        //- old leaves while new slides in - side by side
        //- old stays, new slides on top
        //- sort old/new by some key/score
#pragma warning disable CS0649
        [SerializeField] private DataSlotSlidePresenterOptions _options;
        [SerializeField] private TweenProvider _tweenProvider;
#pragma warning restore CS0649

        private enum State
        {
            FINISHED,
            HIDING_OLD,
            SLIDING,
            SHOWING_NEW,
        }

        private bool _aborted;

        private State _state;

        private ITween _tweenFrom;
        private ITween _tweenTo;
        private IPresenter _animation;

        public override bool IsAnimating => _state != State.FINISHED;

        protected override void DoTransition()
        {
            if (IsAnimating)
                throw new Exception("Still animating!");

            Rect rect = RectTransform.rect;
            float w = rect.width;
            float h = rect.height;

            Vector2 moveOldTo = new Vector2(-w, 0);
            Vector2 moveNewFrom = new Vector2(w, 0);
            float duration = 0.2f;
            AnimationCurve curve = null;

            if (_options != null)
            {
                duration = _options.TransitionDuration;
                curve = _options.TransitionCurve;

                switch (_options.Direction)
                {
                    case DataSlotSlidePresenterOptions.DirectionType.LEFT_RIGHT:
                        moveOldTo = new Vector2(-w, 0);
                        moveNewFrom = new Vector2(w, 0);
                        break;
                    case DataSlotSlidePresenterOptions.DirectionType.RIGHT_LEFT:
                        moveOldTo = new Vector2(w, 0);
                        moveNewFrom = new Vector2(-w, 0);
                        break;
                    case DataSlotSlidePresenterOptions.DirectionType.TOP_DOWN:
                        moveOldTo = new Vector2(0, -h);
                        moveNewFrom = new Vector2(0, h);
                        break;
                    case DataSlotSlidePresenterOptions.DirectionType.BOTTOM_UP:
                        moveOldTo = new Vector2(0, h);
                        moveNewFrom = new Vector2(0, -h);
                        break;
                }
            }


            _nextModel = _model.Data;

            IPresenter fromPresenter = _activePresenter;
            object fromModel = _activeModel;

            IPresenter toPresenter = CreatePresenter(_model.Data);
            object toModel = _model.Data;

            if (fromPresenter != null)
            {
                _suppressActivation = true;
                if (IsActive)
                    fromPresenter.Deactivate();

                _state = State.HIDING_OLD;
                _animation = fromPresenter;
                _animation.AnimateHide(0, () =>
                {
                    _animation = null;
                    CallOnKernelUpdate(Slide);
                });
            }
            else
            {
                Slide();
            }

            void Slide()
            {
                _state = State.SLIDING;

                if (toPresenter != null)
                {
                    //set starting position before binding, so presenter has a correct tm 
                    toPresenter.RectTransform.anchoredPosition = moveNewFrom;
                    BindingHelper.Bind(toModel, toPresenter, this, !_aborted);
                }

                if (_aborted)
                {
                    SlideFinished();
                }
                else
                {
                    ITweenSystem tweenSystem = _tweenProvider.GetTweenSystem();
                    if (fromPresenter != null)
                    {
                        ITween tween = fromPresenter.RectTransform.AnimateAnchoredPosition(tweenSystem, moveOldTo, duration);
                        if (curve != null)
                            tween.SetEase(curve);

                        _tweenFrom = tween;
                    }

                    if (toPresenter != null)
                    {
                        ITween tween = toPresenter.RectTransform.AnimateAnchoredPosition(tweenSystem, Vector2.zero, duration);
                        if (curve != null)
                            tween.SetEase(curve);

                        _tweenTo = tween;
                    }

                    ITween anyTween = _tweenFrom ?? _tweenTo;
                    anyTween.SetCompleteCallback(() =>
                    {
                        _tweenFrom = null;
                        _tweenTo = null;
                        CallOnKernelUpdate(SlideFinished);
                    });
                }
            }

            void SlideFinished()
            {
                _nextModel = null;
                _activeModel = toModel;
                _activePresenter = toPresenter;

                if (fromPresenter != null)
                {
                    BindingHelper.Unbind(fromModel, fromPresenter);

                    PresenterMap.DestroyPresenter(fromModel, fromPresenter);
                }

                if (toPresenter != null && !_aborted)
                {
                    _state = State.SHOWING_NEW;
                    _animation = toPresenter;
                    _animation.AnimateShow(0, Finish); //TODO: schedule?
                }
                else
                {
                    //TODO: reset visibility if aborted?
                    Finish();
                }
            }

            void Finish()
            {
                _nextModel = null;
                _tweenFrom = null;
                _tweenTo = null;
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
            _tweenFrom?.Complete();
            _tweenTo?.Complete();
            ExecuteUpdateCallback();
            _aborted = false;
        }
    }
}