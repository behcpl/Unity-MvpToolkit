using System;
using System.Collections.Generic;
using Behc.Mvp.Components;
using Behc.Mvp.Model;
using Behc.Mvp.Utils;
using UnityEngine;
using Behc.Utils;

// ReSharper disable InconsistentNaming

namespace Behc.Mvp.Presenter
{
    public class AnimatedPresenterBase<T> : DataPresenterBase<T> where T : class, IReactive, IDataCollection
    {
        protected enum ItemState
        {
            ANIMATE_SHOW,
            READY,
            ANIMATE_HIDE,
            READY_TO_REMOVE
        }

        protected class ItemDesc
        {
            public object Model;
            public IPresenter Presenter;
            public ItemState State;
            public bool Active;
        }

        public override bool IsAnimating => _animating;

#pragma warning disable CS0649
        [SerializeField] protected Curtain _curtain;
        [SerializeField] protected int _sortingStep = 100;
#pragma warning restore CS0649

        protected List<ItemDesc> _items = new List<ItemDesc>();
        protected readonly List<ItemDesc> _hidingItems = new List<ItemDesc>();
        protected int _layerId = 0;
        protected bool _animating;

        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);

            Canvas canvas = GetComponent<Canvas>();
            if (canvas.IsNotNull())
                _layerId = canvas.sortingLayerID;

            int order = _sortingStep;
            foreach (object dataModel in _model.Items)
            {
                ItemDesc desc = new ItemDesc
                {
                    Model = dataModel,
                    Presenter = PresenterMap.CreatePresenter(dataModel, RectTransform)
                };
                _items.Add(desc);

                BindingHelper.Bind(desc.Model, desc.Presenter, this, prepareForAnimation);

                if (desc.Presenter is IPresenterSorting sorting)
                    sorting.SetSortingOrder(order, _layerId);

                ApplyPosition(desc.Model, desc.Presenter);

                order += _sortingStep;
            }
        }

        public override void Unbind()
        {
            foreach (ItemDesc itemDesc in _items)
            {
                if (itemDesc.Presenter.IsAnimating)
                    itemDesc.Presenter.AbortAnimations();

                BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);
                PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);
            }

            _items.Clear();

            foreach (ItemDesc itemDesc in _hidingItems)
            {
                if (itemDesc.Presenter.IsAnimating)
                    itemDesc.Presenter.AbortAnimations();

                BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);
                PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);
            }

            _hidingItems.Clear();

            base.Unbind();
        }

        public override void AnimateShow(float startTime, Action onFinish)
        {
            if (_items.Count == 0)
            {
                _animating = false;
                onFinish?.Invoke();
                return;
            }

            _animating = true;
            WhenAll whenAll = new WhenAll();
            whenAll.Setup(() =>
            {
                _animating = false;
                onFinish?.Invoke();
            }, _items.Count);

            for (int i = 0; i < _items.Count; i++)
            {
                int index = i;
                ItemDesc itemDesc = _items[i];

                itemDesc.Presenter.AnimateShow(startTime, () =>
                {
                    itemDesc.State = ItemState.READY;
                    _contentChanged = true;
                    RequestUpdate();
                    whenAll.Completed(index);
                });
            }
        }

        public override void AnimateHide(float startTime, Action onFinish)
        {
            if (_items.Count == 0)
            {
                _animating = false;
                onFinish?.Invoke();
                return;
            }

            _animating = true;
            WhenAll whenAll = new WhenAll();
            whenAll.Setup(() =>
            {
                _animating = false;
                onFinish?.Invoke();
            }, _items.Count);

            for (int i = 0; i < _items.Count; i++)
            {
                int index = i;
                ItemDesc itemDesc = _items[i];

                if (itemDesc.Presenter.IsAnimating)
                    itemDesc.Presenter.AbortAnimations();

                itemDesc.Presenter.AnimateHide(startTime, () =>
                {
                    itemDesc.State = ItemState.READY_TO_REMOVE;
                    _contentChanged = true;
                    RequestUpdate();
                    whenAll.Completed(index);
                });
            }
        }

        public override void AbortAnimations()
        {
            foreach (ItemDesc desc in _items)
            {
                if (desc.Presenter.IsAnimating)
                    desc.Presenter.AbortAnimations();
            }

            foreach (ItemDesc desc in _hidingItems)
            {
                if (desc.Presenter.IsAnimating)
                    desc.Presenter.AbortAnimations();
            }
        }

        public override void Activate()
        {
            base.Activate();

            if (_items.Count > 0)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (topLevel.State == ItemState.READY)
                {
                    topLevel.Presenter.Activate();
                    topLevel.Active = true;
                }
            }
        }

        public override void Deactivate()
        {
            if (_items.Count > 0)
            {
                ItemDesc topLevel = _items[_items.Count - 1];
                if (topLevel.Active)
                {
                    topLevel.Presenter.Deactivate();
                    topLevel.Active = false;
                }
            }

            base.Deactivate();
        }

        protected override void OnScheduledUpdate()
        {
            if (_contentChanged)
            {
                UpdateContent();
                _contentChanged = false;
            }
        }

        protected virtual void UpdateContent()
        {
            List<ItemDesc> oldItems = _items;
            _items = new List<ItemDesc>(_model.ItemsCount);

            int order = _sortingStep;
            foreach (object dataModel in _model.Items)
            {
                ItemDesc desc = oldItems.Find(i => i.Model == dataModel);
                if (desc != null)
                {
                    oldItems.Remove(desc);
                }
                else
                {
                    desc = new ItemDesc
                    {
                        State = ItemState.ANIMATE_SHOW,
                        Model = dataModel,
                        Presenter = PresenterMap.CreatePresenter(dataModel, RectTransform)
                    };
                    BindingHelper.Bind(desc.Model, desc.Presenter, this, true);
                    desc.Presenter.AnimateShow(0, () =>
                    {
                        desc.State = ItemState.READY;
                        _contentChanged = true;
                        RequestUpdate();
                    });

                    ApplyPosition(desc.Model, desc.Presenter);
                }

                _items.Add(desc);

                if (desc.Presenter is IPresenterSorting sorting)
                    sorting.SetSortingOrder(order, _layerId);

                order += _sortingStep;
            }

            foreach (ItemDesc oldDesc in oldItems)
            {
                if (oldDesc.Active)
                    oldDesc.Presenter.Deactivate();

                if (oldDesc.Presenter.IsAnimating)
                    oldDesc.Presenter.AbortAnimations();

                _hidingItems.Add(oldDesc);
                oldDesc.State = ItemState.ANIMATE_HIDE;
                oldDesc.Presenter.AnimateHide(0, () =>
                {
                    oldDesc.State = ItemState.READY_TO_REMOVE;
                    _contentChanged = true;
                    RequestUpdate();
                });
            }

            foreach (ItemDesc hidingDesc in _hidingItems)
            {
                if (hidingDesc.State == ItemState.READY_TO_REMOVE)
                {
                    BindingHelper.Unbind(hidingDesc.Model, hidingDesc.Presenter);
                    PresenterMap.DestroyPresenter(hidingDesc.Model, hidingDesc.Presenter);
                }
            }

            _hidingItems.RemoveAll(i => i.State == ItemState.READY_TO_REMOVE);
        }
    
        protected virtual void ApplyPosition(object itemModel, IPresenter itemPresenter) {}
    }
}