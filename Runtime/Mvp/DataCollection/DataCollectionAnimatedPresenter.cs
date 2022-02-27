using System;
using System.Collections.Generic;
using Behc.Mvp.Components;
using Behc.Mvp.DataCollection.Layout;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using Behc.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace Behc.Mvp.DataCollection
{
    public class DataCollectionAnimatedPresenter : DataPresenterBase<DataCollection>
    {
        private enum State
        {
            UNDEFINED,
            WAITING_FOR_ANIMATE_SHOW,
            ANIMATE_SHOW,
            READY,
            ANIMATE_HIDE,
            WAITING_FOR_DESPAWN,
        }

        private enum ItemState
        {
            UNDEFINED,
            WAITING_FOR_SHOW_ANIMATION_ITEM_PRESENTER,
            WAITING_FOR_SHOW_ANIMATION_ITEM,
            WAITING_FOR_SHOW_ANIMATION_PRESENTER,
            READY,
            MOVING,
            WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER,
            WAITING_FOR_HIDE_ANIMATION_ITEM,
            WAITING_FOR_HIDE_ANIMATION_PRESENTER,
            WAITING_FOR_DESPAWN,
        }

        private class ItemDesc
        {
            public object Id;
            public object Model;
            public IPresenter Presenter;
            public bool Active;
            public ItemState ItemState;

            public Rect PlacementFrom;
            public Rect PlacementTo;

            public float Timer;
            public float Duration;
            public float Delay;
            public AnimationCurve TimeCurve;

            public bool AnimatingRect => ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM ||
                                         ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM ||
                                         ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER ||
                                         ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM_PRESENTER ||
                                         ItemState == ItemState.MOVING;

            public bool AnimatingShow => ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_PRESENTER ||
                                         ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM ||
                                         ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM_PRESENTER;

            public bool AnimatingHide => ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_PRESENTER ||
                                         ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM ||
                                         ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER;
        }

        public override bool IsAnimating => _state == State.ANIMATE_SHOW || _state == State.ANIMATE_HIDE;

#pragma warning disable CS0649
        [SerializeField] private CollectionLayoutOptions _layoutOptions;
        [SerializeField] private DataCollectionAnimatedOptions _animationOptions;

        [SerializeField] private float _safeMargin = 2.0f;
        [SerializeField] private float _fatMargin = 10.0f;

        [SerializeField] private bool _animateItemsOnShow;
        [SerializeField] private bool _animateItemsOnHide;
#pragma warning restore CS0649

        private List<ItemDesc> _itemPresenters = new List<ItemDesc>();
        private readonly List<ItemDesc> _removedItems = new List<ItemDesc>();
        private Dictionary<object, int> _itemIdToIndex = new Dictionary<object, int>();
        private List<Rect> _itemRects = new List<Rect>();

        private ICollectionLayout _layout;

        private Dictionary<Type, IMeasure> _measureItems;
        private Func<object, Vector2> _evaluateSize;
        private Func<object, object, float> _evaluateGap;

        private ViewRegion _viewRegion;

        private Rect _rect;
        private Rect _clipRect;
        private Rect _clipRectFat;

        private State _state;
        private bool _widthChanged;
        private bool _heightChanged;
        private bool _visibilityChanged;

        private Action _onAnimateShowCompleted;
        private Action _onAnimateHideCompleted;

        private const float _THRESHOLD = 1;

        public override void Initialize(PresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            base.Initialize(presenterMap, kernel);

            _layout = _layoutOptions.CreateLayout();
        }

        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);

            RectTransform.ForceUpdateRectTransforms();

            Rect rect = RectTransform.rect;
            _layout.SetContentSize(rect.width, rect.height);

            _viewRegion = GetComponentInParent<ViewRegion>();
            if (_viewRegion != null)
            {
                // Debug.LogWarning($"Bind to {_viewRegion.gameObject.name}, clip: {_viewRegion.ClipRect}");
                UpdateClipRect(_viewRegion.ClipRect, out bool _);
            }

            _state = prepareForAnimation && _animateItemsOnShow ? State.WAITING_FOR_ANIMATE_SHOW : State.READY;
            if (_state == State.READY)
                InitializeContent();
        }

        public override void Unbind()
        {
            AbortAnimations();

            foreach (ItemDesc itemDesc in _removedItems)
            {
                if (itemDesc.Presenter == null)
                    continue;

                Assert.IsFalse(itemDesc.Presenter.IsAnimating, "Still animating!");
                BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);
                PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);
            }

            _removedItems.Clear();

            foreach (ItemDesc itemDesc in _itemPresenters)
            {
                if (itemDesc.Presenter == null)
                    continue;

                Assert.IsFalse(itemDesc.Presenter.IsAnimating, "Still animating!");
                BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);
                PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);
            }

            _itemPresenters.Clear();
            _itemIdToIndex.Clear();
            _itemRects.Clear();

            _viewRegion = null;

            base.Unbind();
        }

        public override void AnimateShow(float startTime, Action onFinish)
        {
            // if(!_animateItemsOnShow) we start with READY state 
            if (_state == State.READY)
            {
                onFinish?.Invoke();
                return;
            }

            Assert.IsTrue(_state == State.WAITING_FOR_ANIMATE_SHOW, "Invalid state!");
            _state = State.ANIMATE_SHOW;
            _onAnimateShowCompleted = onFinish;
            _contentChanged = true;
            RequestUpdate();
        }

        public override void AnimateHide(float startTime, Action onFinish)
        {
            if (!_animateItemsOnHide)
            {
                onFinish?.Invoke();
                return;
            }

            Rect rect = RectTransform.rect;

            _state = State.ANIMATE_HIDE;
            _onAnimateHideCompleted = onFinish;
            RequestUpdate();

            float delay = 0;
            for (int i = 0; i < _itemPresenters.Count; i++)
            {
                ItemDesc itemDesc = _itemPresenters[i];
                if (itemDesc.Presenter != null && itemDesc.Presenter.IsAnimating)
                    itemDesc.Presenter.AbortAnimations();

                itemDesc.ItemState = itemDesc.AnimatingShow && itemDesc.Presenter == null ? ItemState.WAITING_FOR_DESPAWN : ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER;
                itemDesc.Timer = 0;
                itemDesc.Delay = delay;
                itemDesc.Duration = _animationOptions.HideDuration;
                itemDesc.TimeCurve = _animationOptions.HideCurve;

                itemDesc.PlacementFrom = _itemRects[i];
                itemDesc.PlacementTo = new Rect(
                    itemDesc.PlacementFrom.x + _animationOptions.HideOffset.x + _animationOptions.HideWeight.x * rect.width,
                    itemDesc.PlacementFrom.y + _animationOptions.HideOffset.y + _animationOptions.HideWeight.y * rect.height,
                    itemDesc.PlacementFrom.width,
                    itemDesc.PlacementFrom.height);

                if (itemDesc.Presenter != null)
                {
                    if (itemDesc.Active)
                    {
                        itemDesc.Presenter.Deactivate();
                        itemDesc.Active = false;
                    }

                    itemDesc.Presenter.AnimateHide(0, () =>
                    {
                        if (itemDesc.ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER)
                            itemDesc.ItemState = ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM;

                        if (itemDesc.ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_PRESENTER)
                        {
                            itemDesc.ItemState = ItemState.WAITING_FOR_DESPAWN;
                            _visibilityChanged = true;
                            RequestUpdate();
                        }
                    });
                }

                if (itemDesc.Presenter != null) //don't wait for objects that are not visible
                    delay += _animationOptions.HidePropagateDelay;
                _removedItems.Add(itemDesc);
            }

            _itemPresenters.Clear();
            _itemRects.Clear();
            _itemIdToIndex.Clear();
        }

        public override void AbortAnimations()
        {
            foreach (ItemDesc itemDesc in _removedItems)
            {
                if (itemDesc.Presenter == null)
                    continue;

                if (itemDesc.Presenter.IsAnimating)
                {
                    itemDesc.Presenter.AbortAnimations();

                    itemDesc.ItemState = ItemState.WAITING_FOR_DESPAWN;
                    itemDesc.Timer = itemDesc.Delay + itemDesc.Duration;
                }
            }

            foreach (ItemDesc itemDesc in _itemPresenters)
            {
                if (itemDesc.Presenter != null && itemDesc.Presenter.IsAnimating)
                    itemDesc.Presenter.AbortAnimations();

                if (itemDesc.AnimatingShow || itemDesc.ItemState == ItemState.MOVING)
                    itemDesc.ItemState = ItemState.READY;

                if (itemDesc.AnimatingHide)
                    itemDesc.ItemState = ItemState.WAITING_FOR_DESPAWN;

                itemDesc.Timer = itemDesc.Delay + itemDesc.Duration;
            }

            _onAnimateShowCompleted?.Invoke();
            _onAnimateShowCompleted = null;

            _onAnimateHideCompleted?.Invoke();
            _onAnimateHideCompleted = null;
        }

        public override void Activate()
        {
            base.Activate();

            foreach (ItemDesc item in _itemPresenters)
            {
                if (item.ItemState == ItemState.READY && item.Presenter != null)
                {
                    item.Presenter.Activate();
                    item.Active = true;
                }
            }
        }

        public override void Deactivate()
        {
            foreach (ItemDesc item in _itemPresenters)
            {
                if (item.Active)
                {
                    item.Presenter.Deactivate();
                    item.Active = false;
                }
            }

            base.Deactivate();
        }

        protected override void OnScheduledUpdate()
        {
            // Debug.Log($"ScheduledUpdate CC:{_contentChanged} W:{_widthChanged} H:{_heightChanged} VIS:{_visibilityChanged} <<{TestCounter.Counter}>>");

            if (_contentChanged)
            {
                UpdateContent();
                CleanUpAndDespawn();
                _contentChanged = false;
                _widthChanged = false;
                _heightChanged = false;
                _visibilityChanged = false;
            }

            if (_layout.RebuildRequired(_widthChanged, _heightChanged))
            {
                UpdateLayout();
                CleanUpAndDespawn();
                _visibilityChanged = false;
            }

            _widthChanged = false;
            _heightChanged = false;

            if (_visibilityChanged)
            {
                UpdateVisibility();
                CleanUpAndDespawn();
                _visibilityChanged = false;
            }

            if (_state == State.ANIMATE_SHOW)
            {
                //TODO: check for condition if animation completed?
                bool ready = true;
                foreach (ItemDesc itemDesc in _itemPresenters)
                {
                    if (itemDesc.AnimatingShow && itemDesc.Presenter != null)
                    {
                        ready = false;
                        break;
                    }
                }

                if (ready)
                {
                    _state = State.READY;
                    _onAnimateShowCompleted?.Invoke();
                    _onAnimateShowCompleted = null;
                }
            }

            if (_state == State.ANIMATE_HIDE && _removedItems.Count == 0)
            {
                _state = State.WAITING_FOR_DESPAWN;
                _onAnimateHideCompleted?.Invoke();
                _onAnimateHideCompleted = null;
            }
        }

        public void SetMeasure(Func<object, Vector2> evaluateSize, Func<object, object, float> evaluateGap)
        {
            _evaluateSize = evaluateSize;
            _evaluateGap = evaluateGap;
        }

        public void RegisterMeasure(Type type, IMeasure measure)
        {
            _measureItems ??= new Dictionary<Type, IMeasure>();

            _measureItems.Add(type, measure);
        }

        public Vector2 MeasureModel(DataCollection model)
        {
            return _layout.GetApproximatedContentSize(RectTransform.rect.size, model.ItemsCount);
        }

        private void InitializeContent()
        {
            Assert.IsTrue(_itemPresenters.Count == 0 && _itemIdToIndex.Count == 0 && _itemRects.Count == 0, "Should be empty!");

            bool alwaysVisible = _viewRegion.IsNull();
            bool neverVisible = !alwaysVisible && (_clipRect.width <= 0 || _clipRect.height <= 0);

            Rect rect = RectTransform.rect;
            Vector2 initialSize = _layout.GetApproximatedContentSize(rect.size, _model.ItemsCount);
            _layout.SetContentSize(initialSize.x, initialSize.y);

            int index = 0;
            object lastItem = null;
            foreach (object item in _model.Items)
            {
                object id = _model.GetItemId(item);

                EvaluateItemSize(item, lastItem, out Vector2 requestedSize, out float requestedGap);
                Rect itemRect = _layout.EvaluateRect(index, _itemRects, requestedSize, requestedGap);

                ItemDesc itemDesc = new ItemDesc
                {
                    Id = id,
                    Model = item,
                    ItemState = ItemState.READY
                };

                _itemPresenters.Add(itemDesc);
                _itemRects.Add(itemRect);
                _itemIdToIndex.Add(id, index);

                UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);

                lastItem = item;
                index++;
            }

            if (_itemRects.Count > 0)
            {
                Vector2 newSize = _layout.GetOptimalContentSize(RectTransform.rect.size, _itemRects);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
            }
        }

        private void UpdateContent()
        {
            //don't accept changes before any show animation (if enabled) ora during hide/cleanup
            if (_state == State.WAITING_FOR_ANIMATE_SHOW || _state == State.ANIMATE_HIDE || _state == State.WAITING_FOR_DESPAWN)
                return;

            //TODO: add fast path, when nothing changes?
            //TODO: add fast path for clearing/resetting?

            List<ItemDesc> oldItems = _itemPresenters;
            Dictionary<object, int> oldMapping = _itemIdToIndex;
            List<Rect> oldRects = _itemRects;

            _itemPresenters = new List<ItemDesc>();
            _itemIdToIndex = new Dictionary<object, int>();
            _itemRects = new List<Rect>();

            bool alwaysVisible = _viewRegion.IsNull();
            bool neverVisible = !alwaysVisible && (_clipRect.width <= 0 || _clipRect.height <= 0);

            Rect rect = RectTransform.rect;
            Vector2 initialSize = _layout.GetApproximatedContentSize(rect.size, _model.ItemsCount);
            _layout.SetContentSize(initialSize.x, initialSize.y);

            bool hasMovePhase = false;
            int index = 0;
            object lastItem = null;
            float showDelay = 0;
            foreach (object item in _model.Items)
            {
                ItemDesc itemDesc;
                object id = _model.GetItemId(item);

                EvaluateItemSize(item, lastItem, out Vector2 requestedSize, out float requestedGap);
                Rect itemRect = _layout.EvaluateRect(index, _itemRects, requestedSize, requestedGap);
                Rect updateWithRect = itemRect;

                if (oldMapping.TryGetValue(id, out int oldIndex))
                {
                    itemDesc = oldItems[oldIndex];
                    oldMapping.Remove(itemDesc.Id);

                    itemDesc.Id = id;
                    itemDesc.Model = item;

                    Rect oldRect = oldRects[oldIndex];

                    if (SimilarRect(itemRect, oldRect))
                    {
                        if (itemDesc.Presenter != null && (alwaysVisible || !neverVisible && _clipRectFat.Overlaps(itemRect)))
                        {
                            SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.position.x, itemRect.position.y, itemRect.width, itemRect.height);

                            itemDesc.Presenter.Rebind(item);
                        }
                    }
                    else
                    {
                        updateWithRect = oldRect;
                        if (itemDesc.Presenter != null && (alwaysVisible || !neverVisible && _clipRectFat.Overlaps(oldRect)))
                        {
                            SetInsetAndSize(itemDesc.Presenter.RectTransform, oldRect.position.x, oldRect.position.y, oldRect.width, oldRect.height);

                            itemDesc.Presenter.Rebind(item);

                            if (itemDesc.Presenter.IsAnimating) //force finish any spawning animation
                                itemDesc.Presenter.AbortAnimations();

                            if (itemDesc.ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM)
                            {
                                itemDesc.ItemState = ItemState.READY;
                                if (IsActive && !itemDesc.Active)
                                {
                                    itemDesc.Presenter.Activate();
                                    itemDesc.Active = true;
                                }
                            }
                        }

                        itemDesc.ItemState = ItemState.MOVING;
                        itemDesc.Timer = 0;
                        itemDesc.Delay = 0; // move delay
                        itemDesc.Duration = _animationOptions.MoveDuration;
                        itemDesc.TimeCurve = _animationOptions.MoveCurve;

                        itemDesc.PlacementFrom = oldRect;
                        itemDesc.PlacementTo = itemRect;

                        hasMovePhase = true;
                    }
                }
                else
                {
                    Rect showStartingRect = new Rect(
                        itemRect.x + _animationOptions.ShowOffset.x + _animationOptions.ShowWeight.x * rect.width,
                        itemRect.y + _animationOptions.ShowOffset.y + _animationOptions.ShowWeight.y * rect.height,
                        itemRect.width,
                        itemRect.height);
                    updateWithRect = showStartingRect;

                    itemDesc = new ItemDesc
                    {
                        Id = id,
                        Model = item,

                        ItemState = ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM_PRESENTER,
                        Timer = 0,
                        Delay = showDelay,
                        Duration = _animationOptions.ShowDuration,
                        TimeCurve = _animationOptions.ShowCurve,

                        PlacementFrom = showStartingRect,
                        PlacementTo = itemRect
                    };

                    showDelay += _animationOptions.ShowPropagateDelay;
                }

                _itemPresenters.Add(itemDesc);
                _itemRects.Add(itemRect);
                _itemIdToIndex.Add(id, index);

                UpdateItem(itemDesc, alwaysVisible, neverVisible, updateWithRect);

                lastItem = item;
                index++;
            }

            float delayTime = 0;
            foreach (var kv in oldMapping)
            {
                ItemDesc remove = oldItems[kv.Value];
                if (remove.Presenter == null) //ignore already invisible, TODO: with slow animations, hiding element could become visible
                    continue;

                if (remove.Presenter.IsAnimating)
                    remove.Presenter.AbortAnimations();

                if (remove.Active)
                {
                    remove.Presenter.Deactivate();
                    remove.Active = false;
                }

                remove.PlacementFrom = oldRects[kv.Value];
                remove.PlacementTo = new Rect(
                    remove.PlacementFrom.x + _animationOptions.HideOffset.x + _animationOptions.HideWeight.x * rect.width,
                    remove.PlacementFrom.y + _animationOptions.HideOffset.y + _animationOptions.HideWeight.y * rect.height,
                    remove.PlacementFrom.width,
                    remove.PlacementFrom.height);

                remove.ItemState = _animationOptions.HideDuration > 0 ? ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER : ItemState.WAITING_FOR_HIDE_ANIMATION_PRESENTER;
                remove.Timer = 0;
                remove.Delay = delayTime;
                remove.Duration = _animationOptions.HideDuration;
                remove.TimeCurve = _animationOptions.HideCurve;

                remove.Presenter.AnimateHide(0, () =>
                {
                    if (remove.ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER)
                        remove.ItemState = ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM;
                    if (remove.ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_PRESENTER)
                        remove.ItemState = ItemState.WAITING_FOR_DESPAWN;
                });

                if (remove.ItemState == ItemState.WAITING_FOR_DESPAWN)
                {
                    BindingHelper.Unbind(remove.Model, remove.Presenter);
                    PresenterMap.DestroyPresenter(remove.Model, remove.Presenter);
                }
                else
                {
                    _removedItems.Add(remove);
                }

                delayTime += _animationOptions.HidePropagateDelay;
            }

            float delayMovingPhase = _removedItems.Count > 0 ? _animationOptions.MoveAfterHideDelay : 0;
            float delayShowingPhase = hasMovePhase ? _animationOptions.ShowAfterMoveDelay : _removedItems.Count > 0 ? _animationOptions.ShowAfterHideDelay : 0;
            foreach (ItemDesc itemDesc in _itemPresenters)
            {
                if (itemDesc.ItemState == ItemState.MOVING)
                    itemDesc.Delay += delayMovingPhase;
                if (itemDesc.AnimatingShow)
                    itemDesc.Delay += delayShowingPhase;
            }

            if (_itemRects.Count > 0)
            {
                Vector2 newSize = _layout.GetOptimalContentSize(RectTransform.rect.size, _itemRects);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
            }
        }

        private void UpdateLayout()
        {
            bool alwaysVisible = _viewRegion.IsNull();
            bool neverVisible = !alwaysVisible && (_clipRect.width <= 0 || _clipRect.height <= 0);

            _itemRects.Clear();
            Rect rect = RectTransform.rect;
            Vector2 initialSize = _layout.GetApproximatedContentSize(rect.size, _model.ItemsCount);
            _layout.SetContentSize(initialSize.x, initialSize.y);

            for (int i = 0; i < _itemPresenters.Count; i++)
            {
                ItemDesc itemDesc = _itemPresenters[i];
                object lastItem = i > 0 ? _itemPresenters[i - 1].Model : null;

                EvaluateItemSize(itemDesc.Model, lastItem, out Vector2 requestedSize, out float requestedGap);
                Rect itemRect = _layout.EvaluateRect(i, _itemRects, requestedSize, requestedGap);
                Rect updateWithRect = itemRect;

                if (itemDesc.AnimatingRect)
                {
                    updateWithRect = EvaluateRect(itemDesc);
                }

                _itemRects.Add(itemRect);

                if (itemDesc.AnimatingShow && itemDesc.Timer < itemDesc.Delay)
                    continue;

                UpdateItem(itemDesc, alwaysVisible, neverVisible, updateWithRect);

                if (itemDesc.Presenter != null)
                {
                    SetInsetAndSize(itemDesc.Presenter.RectTransform, updateWithRect.position.x, updateWithRect.position.y, updateWithRect.width, updateWithRect.height);
                }
            }

            if (_itemRects.Count > 0)
            {
                Vector2 newSize = _layout.GetOptimalContentSize(RectTransform.rect.size, _itemRects);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSize.x);
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
            }
        }

        private void UpdateVisibility()
        {
            //TODO: should try to animate show/hide if possible
            //we don't known the duration of presenter animation, but we can assume it's same as moving rect duration
            //or just always try to always call animateXX with high offset value (but that will keep animating items forever)

            // Debug.Log($"TestCollectionPresenter::UpdateVisibility <<{TestCounter.Counter}>>");
            bool alwaysVisible = _viewRegion.IsNull();

            bool neverVisible = !alwaysVisible && (_clipRect.width <= 0 || _clipRect.height <= 0);
            for (int i = 0; i < _itemPresenters.Count; i++)
            {
                Rect itemRect = _itemRects[i];
                ItemDesc itemDesc = _itemPresenters[i];

                if (itemDesc.ItemState == ItemState.READY || itemDesc.ItemState == ItemState.WAITING_FOR_DESPAWN)
                {
                    UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);
                    continue;
                }

                if (itemDesc.Timer >= itemDesc.Delay + itemDesc.Duration)
                {
                    if (itemDesc.ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM_PRESENTER)
                        itemDesc.ItemState = ItemState.WAITING_FOR_SHOW_ANIMATION_PRESENTER;

                    if (itemDesc.ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM || itemDesc.ItemState == ItemState.MOVING)
                        itemDesc.ItemState = ItemState.READY;
                }

                itemRect = EvaluateRect(itemDesc);

                if (itemDesc.AnimatingShow && itemDesc.Timer < itemDesc.Delay)
                    continue;

                UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);

                if (itemDesc.Presenter != null)
                {
                    SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.position.x, itemRect.position.y, itemRect.width, itemRect.height);
                }
            }

            foreach (ItemDesc itemDesc in _removedItems)
            {
                if (!itemDesc.AnimatingRect || itemDesc.Timer < itemDesc.Delay)
                    continue;

                if (itemDesc.Timer >= itemDesc.Delay + itemDesc.Duration)
                {
                    if (itemDesc.ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM_PRESENTER)
                        itemDesc.ItemState = ItemState.WAITING_FOR_HIDE_ANIMATION_PRESENTER;

                    if (itemDesc.ItemState == ItemState.WAITING_FOR_HIDE_ANIMATION_ITEM)
                        itemDesc.ItemState = ItemState.WAITING_FOR_DESPAWN;
                }

                Rect itemRect = EvaluateRect(itemDesc);

                UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);

                if (itemDesc.Presenter != null)
                {
                    SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.x, itemRect.y, itemRect.width, itemRect.height);
                }
            }
        }

        private void CleanUpAndDespawn()
        {
            for (int i = _removedItems.Count; i > 0; i--)
            {
                ItemDesc itemDesc = _removedItems[i - 1];
                if (itemDesc.ItemState != ItemState.WAITING_FOR_DESPAWN)
                    continue;

                if (itemDesc.Presenter != null)
                {
                    BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);
                    PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);
                }

                _removedItems[i - 1] = _removedItems[_removedItems.Count - 1];
                _removedItems.RemoveAt(_removedItems.Count - 1);
            }
        }

        private void UpdateItem(ItemDesc itemDesc, bool alwaysVisible, bool neverVisible, Rect itemRect)
        {
            if (itemDesc.Presenter == null)
            {
                if ((neverVisible || !_clipRect.Overlaps(itemRect)) && !alwaysVisible)
                    return;

                if (itemDesc.AnimatingShow && itemDesc.Timer < itemDesc.Delay)
                    return;

                itemDesc.Presenter = PresenterMap.CreatePresenter(itemDesc.Model, RectTransform);
                SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.position.x, itemRect.position.y, itemRect.width, itemRect.height);

                BindingHelper.Bind(itemDesc.Model, itemDesc.Presenter, this, itemDesc.AnimatingShow || itemDesc.AnimatingHide);
                if (itemDesc.AnimatingShow)
                {
                    itemDesc.Presenter.AnimateShow(itemDesc.Timer - itemDesc.Delay, () =>
                    {
                        if (itemDesc.ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM_PRESENTER)
                            itemDesc.ItemState = ItemState.WAITING_FOR_SHOW_ANIMATION_ITEM;

                        if (itemDesc.ItemState == ItemState.WAITING_FOR_SHOW_ANIMATION_PRESENTER)
                        {
                            itemDesc.ItemState = ItemState.READY;

                            if (IsActive && !itemDesc.Active)
                            {
                                itemDesc.Presenter.Activate();
                                itemDesc.Active = true;
                            }
                        }
                    });
                }

                if (itemDesc.AnimatingHide)
                {
                    //TODO:
                }

                if ((itemDesc.ItemState == ItemState.READY || itemDesc.ItemState == ItemState.MOVING) && IsActive && !itemDesc.Active)
                {
                    itemDesc.Presenter.Activate();
                    itemDesc.Active = true;
                }
            }
            else
            {
                if (alwaysVisible || (_clipRectFat.Overlaps(itemRect) && !neverVisible))
                    return;

                if (itemDesc.Presenter.IsAnimating)
                {
                    itemDesc.Presenter.AbortAnimations();
                }

                if (itemDesc.Active)
                {
                    itemDesc.Presenter.Deactivate();
                    itemDesc.Active = false;
                }

                BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);

                PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);

                itemDesc.Presenter = null;
            }
        }

        private void Update()
        {
            // OnRectTransformDimensionsChange() is not enough here, for aggressive optimizations you need to check
            // dimensions separately and check clip rect for movement

            if (_model == null)
                return;

            if (_viewRegion.IsNotNull())
                UpdateClipRect(_viewRegion.ClipRect, out _visibilityChanged);

            Rect rect = RectTransform.rect;
            if (Math.Abs(rect.width - _rect.width) >= _THRESHOLD)
            {
                _widthChanged = true;
                _rect.xMin = rect.xMin;
                _rect.xMax = rect.xMax;
            }

            if (Math.Abs(rect.height - _rect.height) >= _THRESHOLD)
            {
                _heightChanged = true;
                _rect.yMin = rect.yMin;
                _rect.yMax = rect.yMax;
            }

            Animate(Time.smoothDeltaTime); //TODO: unscaled time option

            if (_visibilityChanged || _widthChanged || _heightChanged)
            {
                RequestUpdate();
            }
        }

        private void UpdateClipRect(Rect clipRectWrl, out bool changed)
        {
            Vector2 tl = new Vector2(clipRectWrl.xMin, clipRectWrl.yMin);
            Vector2 br = new Vector2(clipRectWrl.xMax, clipRectWrl.yMax);

            Matrix4x4 inv = RectTransform.localToWorldMatrix.inverse;
            Vector2 ttl = inv.MultiplyPoint(tl);
            Vector2 tbr = inv.MultiplyPoint(br);

            Rect rect = RectTransform.rect;
            Vector2 pivot = RectTransform.pivot;

            Rect newClipRect = Rect.MinMaxRect(
                ttl.x + rect.width * pivot.x - _safeMargin,
                -tbr.y + rect.height * (1.0f - pivot.y) - _safeMargin,
                tbr.x + rect.width * pivot.x + _safeMargin,
                -ttl.y + rect.height * (1.0f - pivot.y) + _safeMargin);

            if (Math.Abs(newClipRect.xMin - _clipRect.xMin) < _THRESHOLD && Math.Abs(newClipRect.yMin - _clipRect.yMin) < _THRESHOLD &&
                Math.Abs(newClipRect.xMax - _clipRect.xMax) < _THRESHOLD && Math.Abs(newClipRect.yMax - _clipRect.yMax) < _THRESHOLD)
            {
                changed = false;
                return;
            }

            changed = true;
            _clipRect = newClipRect;

            _clipRectFat = Rect.MinMaxRect(
                _clipRect.xMin - _fatMargin,
                _clipRect.yMin - _fatMargin,
                _clipRect.xMax + _fatMargin,
                _clipRect.yMax + _fatMargin);
        }

        private void EvaluateItemSize(object item, object lastItem, out Vector2 size, out float gap)
        {
            size = _evaluateSize?.Invoke(item) ?? Vector2.zero;
            if (_measureItems != null && _measureItems.TryGetValue(item.GetType(), out IMeasure measure))
                size = measure.MeasureModel(item);

            gap = _evaluateGap?.Invoke(item, lastItem) ?? -1;
        }

        private static void SetInsetAndSize(RectTransform rt, float offsetLeft, float offsetTop, float width, float height)
        {
            Vector2 pivot = rt.pivot;

            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.sizeDelta = new Vector2(width, height);
            rt.anchoredPosition = new Vector2(offsetLeft + width * pivot.x, -offsetTop - height * (1.0f - pivot.y));
        }

        private void OnDrawGizmosSelected()
        {
            RectTransform rt = (RectTransform)transform;
            Rect rtRect = rt.rect;
            DrawRect(Color.cyan, rt, new Rect(0, 0, rtRect.width, rtRect.height));

            DrawRect(Color.magenta, rt, _clipRect);
            DrawRect(new Color(0.5f, 0, 0.5f), rt, _clipRectFat);

            foreach (Rect rect in _itemRects)
            {
                if (_clipRect.Overlaps(rect))
                    DrawRect(Color.green, rt, rect);
                else if (_clipRectFat.Overlaps(rect))
                    DrawRect(Color.yellow, rt, rect);
                else
                    DrawRect(Color.red, rt, rect);
            }
        }

        private static void DrawRect(Color clr, RectTransform rt, Rect rect)
        {
            Rect parentRect = rt.rect;
            Vector2 parentPivot = rt.pivot;

            Vector3 lMin = new Vector3(rect.xMin - parentRect.width * parentPivot.x, -rect.yMin + parentRect.height * (1.0f - parentPivot.y), 0);
            Vector3 lMax = new Vector3(rect.xMax - parentRect.width * parentPivot.x, -rect.yMax + parentRect.height * (1.0f - parentPivot.y), 0);

            Vector3 wMin = rt.TransformPoint(lMin);
            Vector3 wMax = rt.TransformPoint(lMax);
            Rect wrt = Rect.MinMaxRect(wMin.x, wMin.y, wMax.x, wMax.y);

            Gizmos.color = clr;
            Gizmos.DrawLine(new Vector3(wrt.xMin, wrt.yMin), new Vector3(wrt.xMax, wrt.yMin));
            Gizmos.DrawLine(new Vector3(wrt.xMax, wrt.yMin), new Vector3(wrt.xMax, wrt.yMax));
            Gizmos.DrawLine(new Vector3(wrt.xMax, wrt.yMax), new Vector3(wrt.xMin, wrt.yMax));
            Gizmos.DrawLine(new Vector3(wrt.xMin, wrt.yMax), new Vector3(wrt.xMin, wrt.yMin));
        }

        private void Animate(float dt)
        {
            foreach (ItemDesc itemDesc in _itemPresenters)
            {
                if (itemDesc.ItemState == ItemState.READY || itemDesc.ItemState == ItemState.WAITING_FOR_DESPAWN)
                    continue;

                itemDesc.Timer += dt;
                if (itemDesc.Timer >= itemDesc.Delay)
                    _visibilityChanged = true;

                if (itemDesc.Timer >= itemDesc.Delay + itemDesc.Duration && itemDesc.Presenter == null && itemDesc.AnimatingShow)
                    itemDesc.ItemState = ItemState.READY;
            }

            foreach (ItemDesc itemDesc in _removedItems)
            {
                itemDesc.Timer += dt;
                if (itemDesc.Timer >= itemDesc.Delay)
                    _visibilityChanged = true;

                // ignore presenter hide animation if haven't already started, and time's up
                if (itemDesc.Timer >= itemDesc.Delay + itemDesc.Duration && itemDesc.Presenter == null)
                    itemDesc.ItemState = ItemState.WAITING_FOR_DESPAWN;
            }
        }

        private Rect EvaluateRect(ItemDesc item)
        {
            if (item.Timer < item.Delay)
                return item.PlacementFrom;

            if (item.Timer >= item.Delay + item.Duration)
                return item.PlacementTo;

            float t = (item.Timer - item.Delay) / item.Duration;
            t = item.TimeCurve.Evaluate(t);

            return new Rect
            {
                x = Mathf.LerpUnclamped(item.PlacementFrom.x, item.PlacementTo.x, t),
                y = Mathf.LerpUnclamped(item.PlacementFrom.y, item.PlacementTo.y, t),
                width = Mathf.LerpUnclamped(item.PlacementFrom.width, item.PlacementTo.width, t),
                height = Mathf.LerpUnclamped(item.PlacementFrom.height, item.PlacementTo.height, t),
            };
        }

        private bool SimilarRect(Rect rt1, Rect rt2)
        {
            return Math.Abs(rt1.xMin - rt2.xMin) < _THRESHOLD && Math.Abs(rt1.yMin - rt2.yMin) < _THRESHOLD &&
                   Math.Abs(rt1.xMax - rt2.xMax) < _THRESHOLD && Math.Abs(rt1.yMax - rt2.yMax) < _THRESHOLD;
        }
    }
}