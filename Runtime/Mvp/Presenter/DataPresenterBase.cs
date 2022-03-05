using System;
using System.Collections.Generic;
using Behc.Mvp.Model;
using Behc.Mvp.Utils;
using Behc.Utils;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Behc.Mvp.Presenter
{
    public class DataPresenterBase<T> : MonoBehaviour, IPresenter where T : class, IReactive
    {
        public bool IsActive => _activeSelf && _blockers.Count == 0;

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
        private SmallSet _blockers;
        private bool _activeSelf;

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

            _model = (T)model;
            _updateKernel.RegisterPresenter(this, parent);

            DisposeOnUnbind(_model.Subscribe(ModelChanged));
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

        public void Activate()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#00ff00>Activate</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
            bool wasActive = IsActive;

            Debug.Assert(!_activeSelf, "Already activated!");
            _activeSelf = true;

            if (!wasActive)
                OnActivate();
        }

        public void Deactivate()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#008000>Deactivate</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
            bool wasActive = IsActive;

            Debug.Assert(_activeSelf, "Not activated!");
            _activeSelf = false;
            
            if(wasActive)
                OnDeactivate();
        }

        public void ScheduledUpdate()
        {
            _scheduledUpdate = true;
            OnScheduledUpdate();
            _scheduledUpdate = false;
        }

        public void SetBlockedStatus(bool blocked, object source)
        {
            bool wasActive = IsActive;
            if (blocked)
                _blockers.Add(source);
            else
                _blockers.Remove(source);

            bool active = IsActive;
            if(wasActive && !active)
                OnDeactivate();
            if(!wasActive && active)
                OnActivate();
        }

        protected virtual void OnScheduledUpdate() { }

        protected virtual void OnActivate() { }

        protected virtual void OnDeactivate() { }

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
            Debug.Log($"base ExecuteUpdateCallback cb:{_updateCallback}"); 

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