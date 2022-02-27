using System;
using System.Collections.Generic;
using Behc.Mvp.Model;
using Behc.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Behc.Mvp.Presenter
{
    public class DataPresenterBase<T> : MonoBehaviour, IPresenter where T : class, IReactive
    {
        public bool IsActive { get; protected set; } = false;
        public virtual bool IsAnimating { get; } = false;

        public PresenterMap PresenterMap => _presenterMap;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform.IsNull())
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        protected T _model;
        protected bool _contentChanged;
        protected Action _updateCallback;
        protected bool _scheduledUpdate;

        protected readonly List<IDisposable> _disposeOnUnbind = new List<IDisposable>();

        private RectTransform _rectTransform;
        private PresenterUpdateKernel _updateKernel;
        private PresenterMap _presenterMap;

        public virtual void Initialize(PresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#ff00ff>Initialize</color> <<{PresenterUpdateKernel.Counter}>>");
#endif

            _presenterMap = new PresenterMap(presenterMap);
            _updateKernel = kernel;
        }

        public virtual void Destroy()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#800080>Destroy</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
        }

        public virtual void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            Debug.Assert(_model == null, "Already bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#FF0000>Bind</color> prepare:{prepareForAnimation} <<{PresenterUpdateKernel.Counter}>>");
#endif

            gameObject.SetActive(true);

            _model = (T) model;
            _updateKernel.RegisterPresenter(this, parent);

            DisposeOnUnbind(_model.Subscribe(ModelChanged));
        }

        public virtual void Rebind(object model)
        {
            Debug.Assert(_model != null, "Not bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#CC0000>Rebind</color> <<{PresenterUpdateKernel.Counter}>>");
#endif

            if (!ReferenceEquals(_model, model))
            {
                _disposeOnUnbind.ForEach(d => d.Dispose());
                _disposeOnUnbind.Clear();

                _model = (T) model;
                DisposeOnUnbind(_model.Subscribe(ModelChanged));
            }
            else
            {
                ModelChanged();
            }
        }

        public virtual void Unbind()
        {
            Debug.Assert(_model != null, "Not bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#800000>Unbind</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
           
            if (IsAnimating)
                AbortAnimations();

            gameObject.SetActive(false);

            _updateKernel.UnregisterPresenter(this);

            _disposeOnUnbind.ForEach(d => d.Dispose());
            _disposeOnUnbind.Clear();

            _model = null;
        }

        public virtual void AnimateShow(float startTime, Action onFinish)
        {
            onFinish?.Invoke();
        }

        public virtual void AnimateHide(float startTime, Action onFinish)
        {
            onFinish?.Invoke();
        }

        public virtual void AbortAnimations() { }

        public virtual void Activate()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#00ff00>Activate</color> <<{PresenterUpdateKernel.Counter}>>");
#endif

            Debug.Assert(!IsActive, "Already activated!");
            IsActive = true;
        }

        public virtual void Deactivate()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#008000>Deactivate</color> <<{PresenterUpdateKernel.Counter}>>");
#endif

            Debug.Assert(IsActive, "Not activated!");
            IsActive = false;
        }

        public void ScheduledUpdate()
        {
            _scheduledUpdate = true;
            OnScheduledUpdate();
            _scheduledUpdate = false;
        }
        
        protected virtual void OnScheduledUpdate() {}

        protected void DisposeOnUnbind(IDisposable disposable)
        {
            _disposeOnUnbind.Add(disposable);
        }

        protected void CallOnKernelUpdate(Action updateCallback)
        {
            if (_scheduledUpdate)
            {
                updateCallback?.Invoke();
                return;
            }

            _updateCallback = updateCallback;
            _updateKernel.RequestUpdate(this);
        }

        protected void RequestUpdate()
        {
            _updateKernel.RequestUpdate(this);
        }

        protected void ExecuteUpdateCallback()
        {
            _updateCallback?.Invoke();
            _updateCallback = null;
        }

        protected void ModelChanged()
        {
            _contentChanged = true;
            _updateKernel.RequestUpdate(this);
        }
    }
}