using System;
using System.Collections.Generic;
using System.Linq;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Behc.Mvp.ToastManager
{
    public class ToastManagerPresenter : DataPresenterBase<ToastManager>
    {
        private enum ToastStage
        {
            ANIMATING_SHOW,
            VISIBLE,
            ANIMATING_HIDE,
            FINISHED,
            WAITING_TO_REMOVE,
        }

        private class ToastItem
        {
            public object Model;
            public IPresenter Presenter;
            public ToastStage Stage;
            public float TimeAlive;

            public float LifeTime;
            public Vector2 Velocity;
        }

#pragma warning disable CS0649
        [SerializeField] private float _defaultLifeTime = 3;
        [SerializeField] private Vector2 _defaultVelocity = new Vector2(0, 50);

        [SerializeField] private bool _adjustStartingPosition = true;
        [SerializeField] private float _gapSize = 20;
#pragma warning restore CS0649

        private readonly List<ToastItem> _activeToasts = new List<ToastItem>(16);

        public override void ScheduledUpdate()
        {
            if (_contentChanged)
            {
                UpdateToasts();
            }
        }

        private void Update()
        {
            float advanceTime = Time.smoothDeltaTime;

            foreach (ToastItem item in _activeToasts)
            {
                item.Presenter.RectTransform.anchoredPosition += item.Velocity * advanceTime;

                if (item.Stage != ToastStage.VISIBLE)
                    continue;

                item.TimeAlive += advanceTime;
                if (item.TimeAlive >= item.LifeTime)
                {
                    RequestUpdate();
                }
            }
        }

        private void UpdateToasts()
        {
            foreach (ToastItem item in _activeToasts)
            {
                if (!_model.Toasts.Contains(item.Model))
                {
                    if (item.Presenter.IsAnimating)
                        item.Presenter.AbortAnimations();
                    item.Stage = ToastStage.FINISHED;
                }

                if (item.Stage == ToastStage.VISIBLE && item.TimeAlive >= item.LifeTime)
                {
                    item.Stage = ToastStage.ANIMATING_HIDE;

                    ToastItem toastItem = item;
                    item.Presenter.AnimateHide(0, () =>
                    {
                        toastItem.Stage = ToastStage.FINISHED;
                        RequestUpdate();
                    });
                }

                if (item.Stage == ToastStage.FINISHED)
                {
                    BindingHelper.Unbind(item.Model, item.Presenter);
                    _model.FinishToast(item.Model);
                    PresenterMap.DestroyPresenter(item.Model, item.Presenter);
                    item.Stage = ToastStage.WAITING_TO_REMOVE;
                }
            }

            _activeToasts.RemoveAll(i => i.Stage == ToastStage.WAITING_TO_REMOVE);

            foreach (object toast in _model.Toasts)
            {
                ToastItem item = _activeToasts.Find(i => i.Model == toast);
                if (item != null)
                    continue;

                IPresenter toastPresenter = PresenterMap.CreatePresenter(toast, RectTransform);
                BindingHelper.Bind(toast, toastPresenter, this, true);
                //do not activate, toasts should never have any interaction

                float lifeTime = _defaultLifeTime;
                Vector2 velocity = _defaultVelocity;
                if (toastPresenter is IToastPresenterOptions options)
                {
                    lifeTime = options.LifeTime;
                    velocity = options.Velocity;
                }

                toastPresenter.RectTransform.anchoredPosition = FindBestPosition(toastPresenter, velocity);

                ToastItem toastItem = new ToastItem
                {
                    Model = toast,
                    Presenter = toastPresenter,
                    Stage = ToastStage.ANIMATING_SHOW,
                    TimeAlive = 0,
                    LifeTime = lifeTime,
                    Velocity = velocity
                };

                toastPresenter.AnimateShow(0, () => toastItem.Stage = ToastStage.VISIBLE);
                _activeToasts.Add(toastItem);
            }
        }

        private Vector2 FindBestPosition(IPresenter newPresenter, Vector2 velocity)
        {
            if (_activeToasts.Count > 0 && _adjustStartingPosition)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(newPresenter.RectTransform);
                ToastItem last = _activeToasts[_activeToasts.Count - 1];
                RectTransform lastRt = last.Presenter.RectTransform;
                RectTransform newRt = newPresenter.RectTransform;

                Vector2 adjustedAnchor;
                if (Math.Abs(velocity.y) > Math.Abs(velocity.x))
                {
                    if (velocity.y < 0)
                    {
                        float y = last.Presenter.RectTransform.anchoredPosition.y;
                        y += last.Presenter.RectTransform.rect.height * (1.0f - lastRt.pivot.y);
                        y += _gapSize;
                        y += newPresenter.RectTransform.rect.height * newRt.pivot.y;

                        adjustedAnchor = new Vector2(0, Math.Max(0, y));
                    }
                    else
                    {
                        float y = lastRt.anchoredPosition.y;
                        y -= lastRt.rect.height * lastRt.pivot.y;
                        y -= _gapSize;
                        y -= newRt.rect.height * (1.0f - newRt.pivot.y);

                        adjustedAnchor = new Vector2(0, Math.Min(0, y));
                    }
                }
                else
                {
                    if (velocity.x < 0)
                    {
                        float x = last.Presenter.RectTransform.anchoredPosition.x;
                        x += last.Presenter.RectTransform.rect.width * (1.0f - lastRt.pivot.x);
                        x += _gapSize;
                        x += newPresenter.RectTransform.rect.width * newRt.pivot.x;

                        adjustedAnchor = new Vector2(Math.Max(0, x), 0);
                    }
                    else
                    {
                        float x = last.Presenter.RectTransform.anchoredPosition.y;
                        x -= last.Presenter.RectTransform.rect.width * lastRt.pivot.x;
                        x -= _gapSize;
                        x -= newPresenter.RectTransform.rect.width * (1.0f - newRt.pivot.x);

                        adjustedAnchor = new Vector2(Math.Min(0, x), 0);
                    }
                }

                return adjustedAnchor;
            }

            return Vector2.zero;
        }
    }
}