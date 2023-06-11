using System.Collections.Generic;
using Behc.Mvp.Models;
using Behc.Mvp.Presenters.Layout;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if BEHC_MVPTOOLKIT_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

namespace Behc.Mvp.Presenters
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

        private IPresenter _toolTipPresenter;
        private object _toolTipModel;
        private object _newToolTipModel;

        private float _currentHideTimer;
        private float _newShowTimer;

        //TODO: handle animation
        //TODO: handle multiple show/hide items
        //TODO: (optional) handle deep search for provider (with blocking layers?)
        //TODO: keep outside exclusion rect
        //TODO: snap to source rect option?

        protected override void OnScheduledUpdate()
        {
            if (!_contentChanged || _toolTipModel == _model.CurrentToolTip)
                return;

#if BEHC_MVPTOOLKIT_INPUTSYSTEM
            Vector2 mousePos = Mouse.current.position.ReadValue();
#else
            Vector2 mousePos = Input.mousePosition;
#endif  

            if (_toolTipPresenter != null)
            {
                BindingHelper.Unbind(_toolTipModel, _toolTipPresenter);
                _presenterMap.DestroyPresenter(_toolTipModel, _toolTipPresenter);
                _toolTipPresenter = null;
            }

            _toolTipModel = _model.CurrentToolTip;
            if (_toolTipModel != null)
            {
                _toolTipPresenter = _presenterMap.CreatePresenter(_toolTipModel, RectTransform);
                BindingHelper.Bind(_toolTipModel, _toolTipPresenter, this, false);

                LayoutRebuilder.ForceRebuildLayoutImmediate(_toolTipPresenter.RectTransform);
                UpdateToolTipTransform(_toolTipPresenter.RectTransform, mousePos);
            }
        }

        private void Update()
        {
            if (_model == null)
                return;

#if BEHC_MVPTOOLKIT_INPUTSYSTEM
            Vector2 mousePos = Mouse.current.position.ReadValue();
#else
            Vector2 mousePos = Input.mousePosition;
#endif            
            PointerEventData pointerData = new PointerEventData(_eventSystem);
            pointerData.position = mousePos;

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

            object newModel = newProvider?.GetToolTip(results.Count > 0 ? results[0].gameObject : null);
            if (newModel == _toolTipModel)
            {
                _newToolTipModel = null;
                _currentHideTimer = 0;
                _newShowTimer = 0;
            }
            else if (newModel == _newToolTipModel)
            {
                _currentHideTimer += Time.smoothDeltaTime;
                _newShowTimer += Time.smoothDeltaTime;
            }
            else
            {
                _newToolTipModel = newModel;
            }

            if (_newToolTipModel == null && _currentHideTimer >= _hideDelayTime || _newToolTipModel != null && _newShowTimer >= _showDelayTime)
            {
                _model.SetCurrentToolTip(_newToolTipModel);
            }

            if (_toolTipPresenter != null)
            {
                UpdateToolTipTransform(_toolTipPresenter.RectTransform, mousePos);
            }
        }

        private void UpdateToolTipTransform(RectTransform tm, Vector2 pointerPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, pointerPos, _uiCamera, out Vector2 localPoint);

            LayoutHelper.KeepInsideParentRect(RectTransform, tm, localPoint + _defaultOffset);
        }
    }
}