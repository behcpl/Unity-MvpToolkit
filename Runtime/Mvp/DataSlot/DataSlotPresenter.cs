using System;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Behc.Mvp.DataSlot
{
    public class DataSlotPresenter : DataPresenterBase<DataSlot>
    {
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

        public override void Rebind(object model)
        {
            Debug.Assert(_model != null, "Not bound!");
            // Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) Rebind() <<{PresenterUpdateKernel.Counter}>>");
#endif

            if (!ReferenceEquals(_model, model))
            {
                //here we cannot guarantee that Data is compatible with current presenter
                //probably some optimization opportunity, rebind anyway if new presenter is exactly the same?
                UnbindData();

                _disposeOnUnbind.ForEach(d => d.Dispose());
                _disposeOnUnbind.Clear();

                _model = (DataSlot) model;
                DisposeOnUnbind(_model.Subscribe(ModelChanged));

                BindData();
            }
            else
            {
                ModelChanged();
            }
        }

        public override void Unbind()
        {
            Debug.Assert(_model != null, "Not bound!");

            UnbindData();

            base.Unbind();
        }

        public override void Activate()
        {
            base.Activate();

            if (!_suppressActivation)
                _activePresenter?.Activate();
        }

        public override void Deactivate()
        {
            if (!_suppressActivation)
                _activePresenter?.Deactivate();

            base.Deactivate();
        }

        public override void ScheduledUpdate()
        {
            if (_updateCallback != null)
            {
                Action action = _updateCallback;
                _updateCallback = null;
                action.Invoke();
            }

            if (_activeModel == _model.Data || IsAnimating && _nextModel == _model.Data)
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
            _nextModel = _model.Data;

            IPresenter fromPresenter = _activePresenter;
            object fromModel = _activeModel;

            IPresenter toPresenter = CreatePresenter(_model.Data);
            object toModel = _model.Data;

            _suppressActivation = true;

            if (fromPresenter != null)
            {
                if (IsActive)
                    fromPresenter.Deactivate();

                BindingHelper.Unbind(fromModel, fromPresenter);

                PresenterMap.DestroyPresenter(fromModel, fromPresenter);
            }

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

            IPresenter presenter = PresenterMap.CreatePresenter(_model.Data, RectTransform);
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
            if (_model.Data != null)
            {
                _activeModel = _model.Data;
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
            }
        }
    }
}