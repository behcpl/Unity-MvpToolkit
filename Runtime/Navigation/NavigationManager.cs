using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Behc.Navigation
{
    [Flags]
    public enum NavigationOptions
    {
        DEFAULT = 0, //replace current element, try to reuse context
        KEEP_HISTORY = 0x01,
        RESET_HISTORY = 0x02,
        DONT_REUSE_CONTEXT = 0x04
    }

    public class NavigationManager
    {
        private struct NavigationPoint
        {
            public INavigable Navigable;
            public object Context;
        }

        private readonly List<NavigationPoint> _stack;

        private bool _goBack;
        private INavigable _next;
        private NavigationOptions _nextOptions;
        private object _nextParameters;
        private int _disposingInProgress;
        private Action _continuation;

        public NavigationManager()
        {
            _stack = new List<NavigationPoint>(16);
            _disposingInProgress = -1;
        }

        public void NavigateTo([NotNull] INavigable navigable, [CanBeNull] object parameters, NavigationOptions options)
        {
            ResetDeferredActions();
            _next = navigable;
            _nextOptions = options;
            _nextParameters = parameters;
        }

        public bool NavigateBack()
        {
            if (_stack.Count <= 0)
                return false;

            ResetDeferredActions();
            _goBack = true;
            return true;
        }

        public void Cancel()
        {
            ResetDeferredActions();
        }

        public void CommitDeferredChanges()
        {
            if (_continuation != null)
            {
                _continuation();
                return;
            }

            if (_goBack)
            {
                ResetDeferredActions();
                ProcessGoBack();
            }

            if (_next != null)
            {
                bool keepHistory = (_nextOptions & NavigationOptions.KEEP_HISTORY) != 0;
                bool resetHistory = (_nextOptions & NavigationOptions.RESET_HISTORY) != 0;
                bool dontReuseContext = (_nextOptions & NavigationOptions.DONT_REUSE_CONTEXT) != 0;

                INavigable next = _next;
                object parameters = _nextParameters;

                ResetDeferredActions();

                if (keepHistory || _stack.Count == 0)
                {
                    ProcessPushNew(next, parameters, dontReuseContext);
                }
                else if (resetHistory)
                {
                    ProcessReplaceAll(next, parameters, dontReuseContext);
                }
                else
                {
                    ProcessReplaceCurrent(next, parameters, dontReuseContext);
                }
            }
        }

        private void ProcessGoBack()
        {
            NavigationPoint lastPoint = default;
            if (_stack.Count > 1)
            {
                lastPoint = _stack[_stack.Count - 2];
            }

            NavigationPoint point = _stack[_stack.Count - 1];
            point.Navigable.Pause(point.Context);

            if (IsLastInstance(point.Context))
            {
                _disposingInProgress = _stack.Count - 1;
                point.Navigable.TearDown(point.Context, () =>
                {
                    _stack.RemoveAt(_disposingInProgress);
                    _disposingInProgress = -1;
                });
            }

            if (_disposingInProgress >= 0)
            {
                _continuation = Continue;
            }
            else
            {
                Continue();
            }

            void Continue()
            {
                if (_disposingInProgress >= 0)
                    return;

                lastPoint.Navigable?.Resume(null, lastPoint.Context);
                _continuation = null;
            }
        }

        private void ProcessPushNew(INavigable navigable, object parameters, bool dontReuseContext)
        {
            object context = null;
            if (!dontReuseContext)
            {
                context = GetLastContext(navigable);
            }

            if (context == null)
            {
                navigable.StartUp(parameters, out context);
            }

            navigable.Resume(parameters, context);
            _stack.Add(new NavigationPoint { Context = context, Navigable = navigable });
        }

        private void ProcessReplaceAll(INavigable navigable, object parameters, bool dontReuseContext)
        {
            NavigationPoint lastPoint = _stack[_stack.Count - 1];
            if (lastPoint.Navigable != navigable || dontReuseContext)
            {
                lastPoint.Navigable.Pause(lastPoint.Context);
                lastPoint = default;
            }

            Continue();

            void Continue()
            {
                if (_disposingInProgress >= 0)
                    return;

                while (_stack.Count > 0)
                {
                    NavigationPoint point = _stack[_stack.Count - 1];
                    if (point.Context == lastPoint.Context || !IsLastInstance(lastPoint.Context))
                    {
                        _stack.RemoveAt(_stack.Count - 1);
                        continue;
                    }

                    _disposingInProgress = _stack.Count - 1;
                    point.Navigable.TearDown(point.Context, () =>
                    {
                        _stack.RemoveAt(_disposingInProgress);
                        _disposingInProgress = -1;
                    });

                    if (_disposingInProgress >= 0) //TODO: continue removing all elements
                    {
                        _continuation = Continue;
                        return;
                    }
                }

                _continuation = null;

                if (lastPoint.Navigable != null)
                {
                    lastPoint.Navigable.UpdateParameters(parameters, lastPoint.Context);
                    _stack.Add(lastPoint);
                }
                else
                {
                    ProcessPushNew(navigable, parameters, dontReuseContext);
                }
            }
        }

        private void ProcessReplaceCurrent(INavigable navigable, object parameters, bool dontReuseContext)
        {
            NavigationPoint lastPoint = _stack[_stack.Count - 1];

            if (lastPoint.Navigable == navigable && !dontReuseContext)
            {
                lastPoint.Navigable.UpdateParameters(parameters, lastPoint.Context);
                return;
            }

            lastPoint.Navigable.Pause(lastPoint.Context);

            if (IsLastInstance(lastPoint.Context))
            {
                _disposingInProgress = _stack.Count - 1;
                lastPoint.Navigable.TearDown(lastPoint.Context, () =>
                {
                    _stack.RemoveAt(_disposingInProgress);
                    _disposingInProgress = -1;
                });
            }

            if (_disposingInProgress >= 0)
            {
                _continuation = Continue;
            }
            else
            {
                Continue();
            }

            void Continue()
            {
                if (_disposingInProgress >= 0)
                    return;

                ProcessPushNew(navigable, parameters, dontReuseContext);
                _continuation = null;
            }
        }

        private void ResetDeferredActions()
        {
            _goBack = false;
            _next = null;
            _nextOptions = NavigationOptions.DEFAULT;
            _nextParameters = null;
        }

        private bool IsLastInstance(object context)
        {
            for (int i = _stack.Count - 1; i > 0; i--) //skip last element, it's the one we're testing
            {
                if (_stack[i - 1].Context == context)
                    return false;
            }

            return true;
        }

        private object GetLastContext(INavigable navigable)
        {
            for (int i = _stack.Count; i > 0; i--)
            {
                if (_stack[i - 1].Navigable == navigable)
                    return _stack[i - 1].Context;
            }

            return null;
        }
    }
}