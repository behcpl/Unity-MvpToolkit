using System;
using System.Collections.Generic;
using Behc.Mvp.Components;
using Behc.Mvp.DataCollection.Layout;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;

namespace Behc.Mvp.DataCollection
{
    public class DataCollectionPresenter : DataPresenterBase<DataCollection>
    {
        private class ItemDesc
        {
            public object Id;
            public object Model;
            public IPresenter Presenter;
            public bool Active;
        }

#pragma warning disable CS0649
        [SerializeField] private CollectionLayoutOptions _layoutOptions;

        [SerializeField] private float _safeMargin = 2.0f;
        [SerializeField] private float _fatMargin = 10.0f;
#pragma warning restore CS0649

        private List<ItemDesc> _itemPresenters = new List<ItemDesc>();
        private Dictionary<object, int> _itemIdToIndex = new Dictionary<object, int>();

        private ICollectionLayout _layout;

        private readonly List<Rect> _itemRects = new List<Rect>();

        private Dictionary<Type, IMeasure> _measureItems;
        private Func<object, Vector2> _evaluateSize;
        private Func<object, object, float> _evaluateGap;

        private ViewRegion _viewRegion;

        private Rect _rect;
        private Rect _clipRect;
        private Rect _clipRectFat;

        private bool _widthChanged;
        private bool _heightChanged;
        private bool _visibilityChanged;

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

            UpdateContent();
        }

        public override void Unbind()
        {
            foreach (ItemDesc itemDesc in _itemPresenters)
            {
                if (itemDesc.Presenter == null)
                    continue;

                BindingHelper.Unbind(itemDesc.Model, itemDesc.Presenter);

                PresenterMap.DestroyPresenter(itemDesc.Model, itemDesc.Presenter);
            }

            _itemPresenters.Clear();
            _itemIdToIndex.Clear();
            _itemRects.Clear();

            _viewRegion = null;

            base.Unbind();
        }

        public override void Activate()
        {
            base.Activate();

            foreach (ItemDesc item in _itemPresenters)
            {
                item.Presenter?.Activate();
                item.Active = true;
            }
        }

        public override void Deactivate()
        {
            foreach (ItemDesc item in _itemPresenters)
            {
                if (item.Active)
                {
                    item.Presenter?.Deactivate();
                    item.Active = false;
                }
            }

            base.Deactivate();
        }

        public override void ScheduledUpdate()
        {
            if (_contentChanged)
            {
                UpdateContent();
                _contentChanged = false;
                _widthChanged = false;
                _heightChanged = false;
                _visibilityChanged = false;
            }

            if (_layout.RebuildRequired(_widthChanged, _heightChanged))
            {
                UpdateLayout();
                _visibilityChanged = false;
            }

            _widthChanged = false;
            _heightChanged = false;

            if (_visibilityChanged)
            {
                UpdateVisibility();
                _visibilityChanged = false;
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

        private void UpdateContent()
        {
            // Debug.Log($"TestCollectionPresenter::UpdateContent <<{TestCounter.Counter}>>");
            //TODO: add fast path, when nothing changes?
            //TODO: add fast path for clearing/resetting?

            List<ItemDesc> oldItems = _itemPresenters;
            Dictionary<object, int> oldMapping = _itemIdToIndex;

            _itemPresenters = new List<ItemDesc>();
            _itemIdToIndex = new Dictionary<object, int>();

            _itemRects.Clear();

            bool alwaysVisible = _viewRegion == null;
            bool neverVisible = !alwaysVisible && (_clipRect.width <= 0 || _clipRect.height <= 0);

            Rect rect = RectTransform.rect;
            Vector2 initialSize = _layout.GetApproximatedContentSize(rect.size, _model.ItemsCount);
            _layout.SetContentSize(initialSize.x, initialSize.y);

            int index = 0;
            object lastItem = null;
            foreach (object item in _model.Items)
            {
                ItemDesc itemDesc;
                object id = _model.GetItemId(item);

                EvaluateItemSize(item, lastItem, out Vector2 requestedSize, out float requestedGap);
                Rect itemRect = _layout.EvaluateRect(index, _itemRects, requestedSize, requestedGap);

                if (oldMapping.TryGetValue(id, out int oldIndex2))
                {
                    itemDesc = oldItems[oldIndex2];
                    oldMapping.Remove(itemDesc.Id);

                    itemDesc.Id = id;
                    itemDesc.Model = item;

                    if (itemDesc.Presenter != null && (alwaysVisible || !neverVisible && _clipRectFat.Overlaps(itemRect)))
                    {
                        SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.position.x, itemRect.position.y, itemRect.width, itemRect.height);

                        itemDesc.Presenter.Rebind(item);
                    }
                }
                else
                {
                    itemDesc = new ItemDesc { Id = id, Model = item };
                }

                _itemPresenters.Add(itemDesc);
                _itemRects.Add(itemRect);
                _itemIdToIndex.Add(id, index);

                UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);

                lastItem = item;
                index++;
            }

            foreach (var kv in oldMapping)
            {
                ItemDesc remove = oldItems[kv.Value];
                if (remove.Presenter == null)
                    continue;

                if (remove.Presenter.IsAnimating)
                    remove.Presenter.AbortAnimations();

                if (remove.Active)
                    remove.Presenter.Deactivate();

                BindingHelper.Unbind(remove.Model, remove.Presenter);
                PresenterMap.DestroyPresenter(remove.Model, remove.Presenter);
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
            // Debug.Log($"TestCollectionPresenter::UpdateLayout <<{TestCounter.Counter}>>");
            bool alwaysVisible = _viewRegion == null;
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

                _itemRects.Add(itemRect);

                UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);

                if (itemDesc.Presenter != null)
                {
                    SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.position.x, itemRect.position.y, itemRect.width, itemRect.height);
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
            // Debug.Log($"TestCollectionPresenter::UpdateVisibility <<{TestCounter.Counter}>>");
            bool alwaysVisible = _viewRegion == null;

            bool neverVisible = !alwaysVisible && (_clipRect.width <= 0 || _clipRect.height <= 0);
            for (int i = 0; i < _itemPresenters.Count; i++)
            {
                Rect itemRect = _itemRects[i];
                ItemDesc itemDesc = _itemPresenters[i];

                UpdateItem(itemDesc, alwaysVisible, neverVisible, itemRect);
            }
        }

        private void UpdateItem(ItemDesc itemDesc, bool alwaysVisible, bool neverVisible, Rect itemRect)
        {
            if (itemDesc.Presenter == null)
            {
                if ((neverVisible || !_clipRect.Overlaps(itemRect)) && !alwaysVisible)
                    return;
                itemDesc.Presenter = PresenterMap.CreatePresenter(itemDesc.Model, RectTransform);
                SetInsetAndSize(itemDesc.Presenter.RectTransform, itemRect.position.x, itemRect.position.y, itemRect.width, itemRect.height);
                BindingHelper.Bind(itemDesc.Model, itemDesc.Presenter, this, false);
                if (IsActive)
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

            if (_viewRegion != null)
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
            Rect rtRect = RectTransform.rect;
            DrawRect(Color.cyan, RectTransform, new Rect(0, 0, rtRect.width, rtRect.height));

            DrawRect(Color.magenta, RectTransform, _clipRect);
            DrawRect(new Color(0.5f, 0, 0.5f), RectTransform, _clipRectFat);

            foreach (Rect rect in _itemRects)
            {
                if (_clipRect.Overlaps(rect))
                    DrawRect(Color.green, RectTransform, rect);
                else if (_clipRectFat.Overlaps(rect))
                    DrawRect(Color.yellow, RectTransform, rect);
                else
                    DrawRect(Color.red, RectTransform, rect);
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
    }
}