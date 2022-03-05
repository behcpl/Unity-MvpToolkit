using System;
using Behc.Mvp.Model;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Behc.Mvp.DataSlot
{
    public class DataSlotPresenter : DataPresenterBase<ReactiveModel>, IBlocker
    {
        public event Action<bool, object> OnBlockingStatusChange;

#pragma warning disable CS0649
        [SerializeField] private bool _preserveChildTransform;
#pragma warning restore CS0649

        protected object _activeModel;
        protected object _nextModel;
        protected IPresenter _activePresenter;

        protected bool _suppressActivation;

        public override void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            base.Bind(model, parent, prepareForAnimation);

            BindData();
        }

        public override void Unbind()
        {
            Debug.Assert(_model != null, "Not bound!");

            UnbindData();

            base.Unbind();
        }

        protected override void OnActivate()
        {
            if (!_suppressActivation)
                _activePresenter?.Activate();
        }

        protected override void OnDeactivate()
        {
            if (!_suppressActivation)
                _activePresenter?.Deactivate();
        }

        protected override void OnScheduledUpdate()
        {
            if (_updateCallback != null)
            {
                Action action = _updateCallback;
                _updateCallback = null;
                action.Invoke();
            }

            if (_activeModel == ((IDataSlot)_model).Data || IsAnimating && _nextModel == ((IDataSlot)_model).Data)
            {
                return;
            }

            if (IsAnimating)
            {
                AbortAnimations();
            }

            _updateCallback = null;

            DoTransition();
        }

        protected virtual void DoTransition()
        {
            _nextModel = ((IDataSlot)_model).Data;

            IPresenter fromPresenter = _activePresenter;
            object fromModel = _activeModel;

            IPresenter toPresenter = CreatePresenter(_nextModel);
            object toModel = _nextModel;

            _suppressActivation = true;

            if (fromPresenter != null)
            {
                if (IsActive)
                    fromPresenter.Deactivate();

                BindingHelper.Unbind(fromModel, fromPresenter);

                PresenterMap.DestroyPresenter(fromModel, fromPresenter);
            }

            if (fromModel == null && toModel != null)
                OnBlockingStatusChange?.Invoke(true, this);
            if (fromModel != null && toModel == null)
                OnBlockingStatusChange?.Invoke(false, this);

            if (toPresenter != null)
            {
                BindingHelper.Bind(toModel, toPresenter, this, false);

                if (IsActive)
                    toPresenter.Activate();
            }

            _suppressActivation = false;

            _nextModel = null;
            _activeModel = toModel;
            _activePresenter = toPresenter;
        }

        protected IPresenter CreatePresenter(object model)
        {
            if (model == null)
                return null;

            IPresenter presenter = PresenterMap.CreatePresenter(((IDataSlot)_model).Data, RectTransform);
            if (!_preserveChildTransform)
            {
                RectTransform rt = presenter.RectTransform;
                rt.pivot = Vector2.zero;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;

                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            return presenter;
        }

        private void BindData()
        {
            if (((IDataSlot)_model).Data != null)
            {
                OnBlockingStatusChange?.Invoke(true, this);

                _activeModel = ((IDataSlot)_model).Data;
                _activePresenter = CreatePresenter(_activeModel);
                BindingHelper.Bind(_activeModel, _activePresenter, this, false);
            }
        }

        private void UnbindData()
        {
            if (IsAnimating)
            {
                AbortAnimations();
            }

            if (_activePresenter != null)
            {
                BindingHelper.Unbind(_activeModel, _activePresenter);
                PresenterMap.DestroyPresenter(_activeModel, _activePresenter);

                _activePresenter = null;
                _activeModel = null;

                OnBlockingStatusChange?.Invoke(false, this);
            }
        }
    }
}