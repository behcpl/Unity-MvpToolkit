using System;
using System.Collections.Generic;
using Behc.Utils;
using UnityEngine;
using UnityEngine.Assertions;

namespace Behc.Mvp.Presenters
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

        private PresenterUpdateKernel _updateKernel;
        private PresenterMap _presenterMap;

        public RectTransform RectTransform => (RectTransform)transform;

        public bool IsActive => _active;
        public bool IsAnimating { get; private set; }

        public virtual void Initialize(PresenterMap presenterMap, PresenterUpdateKernel kernel)
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#ff00ff>Initialize</color> <<{PresenterUpdateKernel.Counter}>>");
#endif

            _updateKernel = kernel;
            _presenterMap = presenterMap;

            OnInitialize();
        }

        public virtual void Destroy()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#800080>Destroy</color> <<{PresenterUpdateKernel.Counter}>>");
#endif

            _presenters.ForEach(pf => pf.Presenter.Destroy());

            _disposeOnDestroy.ForEach(d => d.Dispose());
            _disposeOnDestroy.Clear();
        }

        public void Bind(object model, IPresenter parent, bool prepareForAnimation)
        {
            Debug.Assert(_model == null, "Already bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#FF0000>Bind</color> prepare:{prepareForAnimation} <<{PresenterUpdateKernel.Counter}>>");
#endif
    
            gameObject.SetActive(true);
            Debug.Assert(gameObject.activeInHierarchy, "Not active in hierarchy!");

            _model = (T) model;
            _updateKernel.RegisterPresenter(this, parent);

            OnBind(prepareForAnimation);

            _presenters.ForEach(pf => pf.Presenter.Bind(pf.ModelSelector(_model), this, prepareForAnimation));
        }

        public void Unbind()
        {
            Debug.Assert(_model != null, "Not bound!");
            Debug.Assert(_updateKernel.UpdateLoop, "Not in kernel update loop");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#800000>Unbind</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
          
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
            
            Assert.IsFalse(IsAnimating, "Presenter is still animating, check OnAbortAnimations()");
        }

        public virtual void Activate()
        {
            Debug.Assert(!_active, "Already activated!");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#00ff00>Activate</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
            _active = true;

            OnActivate();

            _presenters.ForEach(pf => pf.Presenter.Activate());
        }

        public virtual void Deactivate()
        {
            Debug.Assert(_active, "Not activated!");

#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) <color=#008000>Deactivate</color> <<{PresenterUpdateKernel.Counter}>>");
#endif
            _active = false;

            OnDeactivate();

            _presenters.ForEach(pf => pf.Presenter.Deactivate());
        }

        public virtual void ScheduledUpdate()
        {
#if BEHC_MVPTOOLKIT_VERBOSE
            Debug.Log($"({name}) ScheduledUpdate <<{PresenterUpdateKernel.Counter}>>");
#endif
        }

        // Automatically disposes an object when presenter is no longer needed.
        protected void DisposeOnUnbind(IDisposable disposable)
        {
            _disposeOnUnbind.Add(disposable);
        }

        // Automatically disposes an object when presenter is destroyed.
        protected void DisposeOnDestroy(IDisposable disposable)
        {
            _disposeOnDestroy.Add(disposable);
        }
        
        // Called only once, override to perform some initialization, i.e. RegisterPresenter
        protected virtual void OnInitialize() { }

        protected virtual void OnBind(bool prepareForAnimation) { }
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

        // Create composite presenter from multiple presenters. modelSelector must point to valid model (that subPresenter expects)
        protected void RegisterPresenter(Func<T, object> modelSelector, IPresenter subPresenter)
        {
            subPresenter.Initialize(_presenterMap, _updateKernel);
            _presenters.Add(new PresenterField { Presenter = subPresenter, ModelSelector = modelSelector });
        }

        // Call to request ScheduledUpdate from PresenterUpdateKernel
        protected void RequestUpdate()
        {
            _updateKernel.RequestUpdate(this);
        }
    }
}