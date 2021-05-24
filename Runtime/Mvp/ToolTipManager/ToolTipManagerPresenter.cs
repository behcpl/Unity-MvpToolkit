using System.Collections.Generic;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
#if BEHC_MVPTOOLKIT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace Behc.Mvp.ToolTipManager
{
    public class ToolTipManagerPresenter : DataPresenterBase<ToolTipManager>
    {
#pragma warning disable CS0649
        [SerializeField] private EventSystem _eventSystem;
        [SerializeField] private Camera _uiCamera;

        [SerializeField] private Vector2 _defaultOffset = new Vector2(20, -30);
        [SerializeField] private float _showDelayTime = 5.0f / 60.0f;
        [SerializeField] private float _hideDelayTime = 6.0f / 60.0f;
#pragma warning restore CS0649

        private IToolTipProvider _currentToolTipProvider;
        private float _currentHideTimer;

        private IToolTipProvider _newToolTipProvider;
        private float _newShowTimer;

        private IPresenter _toolTipPresenter;
        private object _toolTipModel;

        //TODO: handle animation
        //TODO: handle multiple show/hide items
        //TODO: (optional) handle deep search for provider (with blocking layers?)
        //TODO: keep outside exclusion rect
        //TODO: snap to source rect option?

        public override void ScheduledUpdate()
        {
            if (!_contentChanged || _toolTipModel == _model.CurrentToolTip)
                return;

            if (_toolTipPresenter != null)
            {
                BindingHelper.Unbind(_toolTipModel, _toolTipPresenter);
                PresenterMap.DestroyPresenter(_toolTipModel, _toolTipPresenter);
                _toolTipPresenter = null;
            }

            _toolTipModel = _model.CurrentToolTip;
            if (_toolTipModel != null)
            {
                _toolTipPresenter = PresenterMap.CreatePresenter(_toolTipModel, RectTransform);
                BindingHelper.Bind(_toolTipModel, _toolTipPresenter, this, false);

                LayoutRebuilder.ForceRebuildLayoutImmediate(_toolTipPresenter.RectTransform);
                UpdateToolTipTransform(_toolTipPresenter.RectTransform, Mouse.current.position.ReadValue());
            }
        }

        private void Update()
        {
            if (_model == null)
                return;

            PointerEventData pointerData = new PointerEventData(_eventSystem);
#if BEHC_MVPTOOLKIT_INPUTSYSTEM
            pointerData.position = Mouse.current.position.ReadValue();
#else
            pointerData.position = Input.mousePosition;
#endif

            List<RaycastResult> results = new List<RaycastResult>();
            _eventSystem.RaycastAll(pointerData, results);

            IToolTipProvider newProvider = null;
            foreach (RaycastResult result in results)
            {
                IPresenter newPresenter = result.gameObject.GetComponent<IPresenter>();
                newProvider = newPresenter as IToolTipProvider;

                if (newPresenter != null)
                    break;
            }

            if (newProvider == _currentToolTipProvider)
            {
                _newToolTipProvider = null;
                _currentHideTimer = 0;
                _newShowTimer = 0;
            }
            else if (newProvider == _newToolTipProvider)
            {
                _currentHideTimer += Time.smoothDeltaTime;
                _newShowTimer += Time.smoothDeltaTime;
            }
            else
            {
                _newToolTipProvider = newProvider;
            }

            if (_newToolTipProvider == null && _currentHideTimer >= _hideDelayTime || _newToolTipProvider != null && _newShowTimer >= _showDelayTime)
            {
                _currentToolTipProvider = newProvider;
                _model.SetCurrentToolTip(_currentToolTipProvider?.GetToolTip());
            }

            if (_toolTipPresenter != null)
                UpdateToolTipTransform(_toolTipPresenter.RectTransform, Mouse.current.position.ReadValue());
        }

        private void UpdateToolTipTransform(RectTransform tm, Vector2 pointerPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, pointerPos, _uiCamera, out Vector2 localPoint);

            LayoutHelper.KeepInsideParentRect(RectTransform, tm, localPoint + _defaultOffset);
        }
    }
}