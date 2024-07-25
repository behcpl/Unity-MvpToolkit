using System.Collections.Generic;
using Behc.Mvp.Models;
using Behc.Mvp.Presenters.Layout;
using Behc.Mvp.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if BEHC_MVPTOOLKIT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

namespace Behc.Mvp.Presenters
{
    public class PopupMenuManagerPresenter : AnimatedPresenterBase<PopupMenuManager>
    {
#pragma warning disable CS0649
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Camera _uiCamera;
#pragma warning restore CS0649

        private IPopupMenuProvider _startedWithProvider;

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
        }

        protected override void OnActivate()
        {
            foreach (ItemDesc desc in _items)
            {
                if (desc.Active || desc.State != ItemState.READY)
                    continue;
                
                desc.Presenter.Activate();
                desc.Active = true;
            }
        }

        protected override void OnDeactivate()
        {
            foreach (ItemDesc desc in _items)
            {
                if (!desc.Active)
                    continue;
                
                desc.Presenter.Deactivate();
                desc.Active = true;
            }
        }

        protected override void UpdateContent()
        {
            int oldCount = _items.Count;
            base.UpdateContent();

            foreach (ItemDesc desc in _items)
            {
                if (desc.Active || desc.State != ItemState.READY)
                    continue;
                
                desc.Presenter.Activate();
                desc.Active = true;
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
        }

        private void CurtainClicked()
        {
            _model.RemoveAll();
        }

        protected override void ApplyPosition(object itemModel, IPresenter itemPresenter)
        {
            itemPresenter.RectTransform.pivot = new Vector2(0, 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(itemPresenter.RectTransform);

            PopupMenuManager.Placement placement = _model.GetItemPlacement(itemModel);
            if (placement == PopupMenuManager.Placement.PLACE_AT_CURSOR)
            {
#if BEHC_MVPTOOLKIT_INPUTSYSTEM
                Vector2 mousePos = Pointer.current.position.ReadValue();
#else
                Vector2 mousePos = Input.mousePosition;
#endif
                RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, mousePos, _uiCamera, out Vector2 localOrigin);
                LayoutHelper.KeepInsideParentRect(RectTransform, itemPresenter.RectTransform, localOrigin);
            }
            else
            {
                Rect wrlRt = _model.GetItemRect(itemModel);
                Vector2 separation = Vector2.zero;
                if (itemPresenter is IPopupMenuPresenterOptions presenterOptions)
                    separation = presenterOptions.Separation;
                    
                Vector2 p1 = RectTransform.InverseTransformPoint(wrlRt.min);
                Vector2 p2 = RectTransform.InverseTransformPoint(wrlRt.max);
                LayoutHelper.AdjacentRect(RectTransform, itemPresenter.RectTransform, Rect.MinMaxRect(p1.x, p1.y, p2.x, p2.y), separation, GetAlignment(placement));
            }
        }

        private void Update()
        {
            if (_model == null)
                return;

#if BEHC_MVPTOOLKIT_INPUTSYSTEM
            Vector2 mousePos = Pointer.current.position.ReadValue();
            bool rmbDown = false;
            bool rmbUp = false;
            if (Pointer.current is Mouse mouse)
            {
                 rmbDown = mouse.rightButton.wasPressedThisFrame;
                 rmbUp = mouse.rightButton.wasReleasedThisFrame;
            }
#else
            Vector2 mousePos = Input.mousePosition;
            bool rmbDown = Input.GetMouseButtonDown(1);
            bool rmbUp = Input.GetMouseButtonUp(1);
#endif
            if (!rmbDown && !rmbUp)
                return;

            PointerEventData pointerData = new PointerEventData(_eventSystem);
            pointerData.position = mousePos;

            List<RaycastResult> results = new List<RaycastResult>();
            _eventSystem.RaycastAll(pointerData, results);

            IPopupMenuProvider newProvider = null;
            foreach (RaycastResult result in results)
            {
                IPresenter newPresenter = result.gameObject.GetComponent<IPresenter>();
                newProvider = newPresenter as IPopupMenuProvider;

                if (newPresenter != null)
                    break;
            }

            if (rmbDown)
            {
                _startedWithProvider = newProvider;
                return;
            }

            if (_startedWithProvider == newProvider)
            {
                object menu = newProvider?.GetPopupMenu();
                if (menu != null)
                {
                    _model.RemoveAll();
                    _model.Add(menu);
                }
            }

            _startedWithProvider = null;
        }

        private static LayoutHelper.Alignment GetAlignment(PopupMenuManager.Placement placement)
        {
            switch (placement)
            {
                case PopupMenuManager.Placement.RIGHT_OF_RECT:
                    return LayoutHelper.Alignment.RIGHT;
                case PopupMenuManager.Placement.LEFT_OF_RECT:
                    return LayoutHelper.Alignment.LEFT;
                case PopupMenuManager.Placement.TOP_OF_RECT:
                    return LayoutHelper.Alignment.TOP;
                case PopupMenuManager.Placement.BOTTOM_OF_RECT:
                    return LayoutHelper.Alignment.BOTTOM;
                default:
                    return LayoutHelper.Alignment.RIGHT;
            }
        }
    }
}