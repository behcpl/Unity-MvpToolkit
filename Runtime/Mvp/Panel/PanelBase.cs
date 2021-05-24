using System;
using System.Collections.Generic;
using Behc.Mvp.Presenter;
using Behc.Mvp.Utils;
using UnityEngine;

namespace Behc.Mvp.Panel
{
    public class PanelBase<T> : MonoBehaviour, IPresenter where T : class
    {
        private class PresenterField
        {
            public IPresenter Presenter;
            public Func<T, object> ModelSelector;
        }

        protected T _model;
        protected bool _active;

        private readonly List<PresenterField> _presenters = new List<PresenterField>();
        private readonly List<IDisposable> _disposeOnDestroy = new List<IDisposable>();
        private readonly List<IDisposable> _disposeOnUnbind = new List<IDisposable>();

        private RectTransform _rectTransform;
        private PresenterUpdateKernel _updateKernel;
        private PresenterMap _presenterMap;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public bool IsActive => _active;
        public bool IsAnimating { get; private set; }

        public virtual void Initialize(PresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
            Debug.Log($"({name}) <color=#ff00ff>Initialize</color> <<{TestCounter.Counter}>>");

            _updateKernel = kernel;
            _presenterMap = presenterMap;

            OnInitialize();
        }

        public virtual void Destroy()
        {
            Debug.Log($"({name}) <color=#800080>Destroy</color> <<{TestCounter.Counter}>>");

            _presenters.ForEach(pf => pf.Presenter.Destroy());

            _disposeOnDestroy.ForEach(d => d.Dispose());
            _disposeOnDestroy.Clear();
        }

        public void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            Debug.Assert(_model == null, "Already bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

            Debug.Log($"({name}) <color=#FF0000>Bind</color> prepare:{prepareForAnimation} <<{TestCounter.Counter}>>");
    
            gameObject.SetActive(true);
            Debug.Assert(gameObject.activeInHierarchy, "Not active in hierarchy!");

            _model = (T) model;
            _updateKernel.RegisterPresenter(this, parent);

            OnBind(prepareForAnimation);

            _presenters.ForEach(pf => pf.Presenter.Bind(pf.ModelSelector(_model), this, prepareForAnimation));
        }

        public void Rebind(object model)
        {
            Debug.Assert(_model != null, "Not bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");
            
            Debug.Log($"({name}) <color=#CC0000>Rebind</color> <<{TestCounter.Counter}>>");

            if (_model != model)
            {
                _model = (T) model;

                OnUnbind();

                _disposeOnUnbind.ForEach(d => d.Dispose());
                _disposeOnUnbind.Clear();
                
                _presenters.ForEach(pf => pf.Presenter.Rebind(pf.ModelSelector(_model)));
        
                OnBind(false);
            }
            else
            {
                OnRebind();
            }
        }

        public void Unbind()
        {
            Debug.Assert(_model != null, "Not bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

            Debug.Log($"({name}) <color=#800000>Unbind</color> <<{TestCounter.Counter}>>");
          
            gameObject.SetActive(false); //TODO: this is optimization, check if some operations requires object to be still active

            _presenters.ForEach(pf => pf.Presenter.Unbind());

            OnUnbind();
            _updateKernel.UnregisterPresenter(this);

            _disposeOnUnbind.ForEach(d => d.Dispose());
            _disposeOnUnbind.Clear();

            _model = null;
        }

        public void AnimateShow(float startTime, Action onFinish)
        {
            IsAnimating = true;
            WhenAll whenAll = new WhenAll();
            whenAll.Setup(() =>
            {
                IsAnimating = false;
                onFinish?.Invoke();
            }, 1 + _presenters.Count);
            OnAnimateShow(startTime, () => whenAll.Completed(0));

            for (int i = 0; i < _presenters.Count; i++)
            {
                int index = i + 1;
                _presenters[i].Presenter.AnimateShow(startTime, () => whenAll.Completed(index));
            }
        }

        public void AnimateHide(float startTime, Action onFinish)
        {
            IsAnimating = true;
            WhenAll whenAll = new WhenAll();
            whenAll.Setup(() =>
            {
                IsAnimating = false;
                onFinish?.Invoke();
            }, 1 + _presenters.Count);

            OnAnimateHide(startTime, () => whenAll.Completed(0));

            for (int i = 0; i < _presenters.Count; i++)
            {
                int index = i + 1;
                _presenters[i].Presenter.AnimateHide(startTime, () => whenAll.Completed(index));
            }
        }

        public void AbortAnimations()
        {
            OnAbortAnimations();

            _presenters.ForEach(pf => pf.Presenter.AbortAnimations());
        }

        public virtual void Activate()
        {
            Debug.Assert(!_active, "Already activated!");

            Debug.Log($"({name}) <color=#00ff00>Activate</color> <<{TestCounter.Counter}>>");
            _active = true;

            OnActivate();

            _presenters.ForEach(pf => pf.Presenter.Activate());
        }

        public virtual void Deactivate()
        {
            Debug.Assert(_active, "Not activated!");

            Debug.Log($"({name}) <color=#008000>Deactivate</color> <<{TestCounter.Counter}>>");
            _active = false;

            OnDeactivate();

            _presenters.ForEach(pf => pf.Presenter.Deactivate());
        }

        public virtual void ScheduledUpdate()
        {
            Debug.Log($"({name}) ScheduledUpdate <<{TestCounter.Counter}>>");
        }

        protected void DisposeOnUnbind(IDisposable disposable)
        {
            _disposeOnUnbind.Add(disposable);
        }

        protected void DisposeOnDestroy(IDisposable disposable)
        {
            _disposeOnDestroy.Add(disposable);
        }
        
        protected virtual void OnInitialize() { }

        protected virtual void OnBind(bool prepareForAnimation) { }
        protected virtual void OnRebind() {}
        protected virtual void OnUnbind() { }

        //Must call onFinish as a last action
        protected virtual void OnAnimateShow(float startTime, Action onFinish)
        {
            onFinish?.Invoke();
        }

        //Must call onFinish as a last action
        protected virtual void OnAnimateHide(float startTime, Action onFinish)
        {
            onFinish?.Invoke();
        }

        protected virtual void OnAbortAnimations() { }

        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }

        protected void RegisterPresenter(Func<T, object> modelSelector, IPresenter subPresenter)
        {
            subPresenter.Initialize(_presenterMap, _updateKernel);
            _presenters.Add(new PresenterField { Presenter = subPresenter, ModelSelector = modelSelector });
        }

        protected void RequestUpdate()
        {
            _updateKernel.RequestUpdate(this);
        }
    }
}