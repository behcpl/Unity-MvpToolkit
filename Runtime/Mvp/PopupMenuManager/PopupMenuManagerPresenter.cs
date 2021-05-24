using System;
using System.Collections.Generic;
using System.Linq;
using Behc.Mvp.Components;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;

namespace Behc.Mvp.PopupMenuManager
{
    public class PopupMenuManagerPresenter : AnimatedPresenterBase<PopupMenuManager>
    {
        public enum Alignment
        {
            DEFAULT,
            RIGHT,
            LEFT,
            TOP,
            BOTTOM,
        }

#pragma warning disable CS0649
        [SerializeField] private Alignment _defaultAlignment = Alignment.RIGHT;
#pragma warning restore CS0649

        private class ItemOptions
        {
            public bool UseRect;
            public Vector2 Origin;
            public Rect OwnerRect;
            public Vector2 Separation;
            public Alignment Alignment;
        }

        private readonly Dictionary<object, ItemOptions> _itemOptions = new Dictionary<object, ItemOptions>(16);

        public void ShowMenu(object parentMenu, object subMenu, Vector2 origin)
        {
            _itemOptions.Add(subMenu, new ItemOptions { UseRect = false, Origin = origin });

            if (parentMenu != null)
                _model.RemoveAfter(parentMenu);
            _model.Add(subMenu);
        }

        public void ShowMenu(object parentMenu, object subMenu, Rect ownerRect, Vector2 separation, Alignment alignment = Alignment.DEFAULT)
        {
            _itemOptions.Add(subMenu, new ItemOptions
            {
                UseRect = true,
                OwnerRect = ownerRect,
                Separation = separation,
                Alignment = alignment == Alignment.DEFAULT ? _defaultAlignment : alignment
            });

            if (parentMenu != null)
                _model.RemoveAfter(parentMenu);
            _model.Add(subMenu);
        }

        public void HideSubMenu(object parentMenu)
        {
            _model.RemoveAfter(parentMenu);
        }

        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);
  
            if (_curtain != null)
            {
                DisposeOnUnbind(_curtain.OnTrigger.Subscribe(CurtainClicked));

                if (prepareForAnimation && _items.Count > 0)
                {
                    _curtain.Show(0);
                }
                else
                {
                    _curtain.Setup(_items.Count > 0, 0);
                }
            }

            ClearOptions();
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

        protected override void UpdateContent()
        {
            int oldCount = _items.Count;
            base.UpdateContent();

            foreach (ItemDesc desc in _items)
            {
                if (!desc.Active && desc.State == ItemState.READY)
                {
                    desc.Presenter.Activate();
                    desc.Active = true;
                }
            }

            if (_curtain != null)
            {
                if (_items.Count == 0)
                {
                    if (oldCount > 0)
                        _curtain.Hide();
                }
                else
                {
                    if (oldCount == 0)
                        _curtain.Show(0);
                }
            }

            ClearOptions();
        }

        private void CurtainClicked()
        {
            _model.RemoveAll();
        }

        private void ClearOptions()
        {
            foreach (object key in _itemOptions.Keys.ToArray())
            {
                if (_model.Items.Contains(key))
                    continue;

                _itemOptions.Remove(key);
            }
        }

        protected override void ApplyPosition(object itemModel, IPresenter itemPresenter)
        {
            if (!_itemOptions.TryGetValue(itemModel, out ItemOptions options))
            {
                Debug.LogWarning($"No options for {itemModel.GetType().Name} using {itemPresenter.RectTransform.gameObject.name} presenter!");

                itemPresenter.RectTransform.anchoredPosition = Vector2.zero;
                return;
            }

            itemPresenter.RectTransform.pivot = new Vector2(0, 1);

            if (options.UseRect)
            {
                Vector2 p1 = RectTransform.InverseTransformPoint(options.OwnerRect.min);
                Vector2 p2 = RectTransform.InverseTransformPoint(options.OwnerRect.max);
                LayoutHelper.AdjacentRect(RectTransform, itemPresenter.RectTransform, Rect.MinMaxRect(p1.x, p1.y, p2.x, p2.y), options.Separation, options.Alignment);
            }
            else
            {
                Vector2 localOrigin = RectTransform.InverseTransformPoint(options.Origin);
                LayoutHelper.KeepInsideParentRect(RectTransform, itemPresenter.RectTransform, localOrigin);
            }
        }
    }
}