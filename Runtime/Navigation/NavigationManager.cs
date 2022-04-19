using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

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
            public string Name;
            public INavigable Navigable;
        }

        private readonly IFactory<string, object, INavigable> _navigableFactory;
        private readonly List<NavigationPoint> _stack;

        private bool _goBack;
        private string _next;
        private NavigationOptions _nextOptions;
        private object _nextParameters;
        private int _disposingInProgress;
        private Action _continuation;

        public NavigationManager(IFactory<string, object, INavigable> navigableFactory)
        {
            _navigableFactory = navigableFactory;
            _stack = new List<NavigationPoint>(16);
            _disposingInProgress = -1;
        }

        public void NavigateTo([NotNull] string name, [CanBeNull] object parameters, NavigationOptions options)
        {
            ResetDeferredActions();
            _next = name;
            _nextOptions = options;
            _nextParameters = parameters;
        }

        public bool NavigateBack()
        {
            if (_stack.Count <= 1)
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

                string next = _next;
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
            point.Navigable.Stop();

            if (IsLastInstance(point.Navigable))
            {
                _disposingInProgress = _stack.Count - 1;
                point.Navigable.LongDispose(() =>
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

                lastPoint.Navigable?.Start();
                _continuation = null;
            }
        }

        private void ProcessPushNew(string name, object parameters, bool dontReuseContext)
        {
            INavigable navigable = null;
            if (!dontReuseContext)
            {
                navigable = GetLastContext(name);
            }

            navigable ??= _navigableFactory.Create(name, parameters);
            navigable.Start();
            
            _stack.Add(new NavigationPoint { Name = name, Navigable = navigable });
        }

        private void ProcessReplaceAll(string name, object parameters, bool dontReuseContext)
        {
            NavigationPoint lastPoint = _stack[_stack.Count - 1];
            if (lastPoint.Name != name || dontReuseContext)
            {
                lastPoint.Navigable.Stop();
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
                    if (point.Navigable == lastPoint.Navigable || !IsLastInstance(lastPoint.Navigable))
                    {
                        _stack.RemoveAt(_stack.Count - 1);
                        continue;
                    }

                    _disposingInProgress = _stack.Count - 1;
                    point.Navigable.LongDispose(() =>
                    {
                        _stack.RemoveAt(_disposingInProgress);
                        _disposingInProgress = -1;
                    });

                    if (_disposingInProgress >= 0)
                    {
                        _continuation = Continue;
                        return;
                    }
                }

                _continuation = null;

                if (lastPoint.Navigable != null)
                {
                    lastPoint.Navigable.Restart(parameters);
                    _stack.Add(lastPoint);
                }
                else
                {
                    ProcessPushNew(name, parameters, dontReuseContext);
                }
            }
        }

        private void ProcessReplaceCurrent(string name, object parameters, bool dontReuseContext)
        {
            NavigationPoint lastPoint = _stack[_stack.Count - 1];

            if (lastPoint.Name == name && !dontReuseContext)
            {
                lastPoint.Navigable.Restart(parameters);
                return;
            }

            lastPoint.Navigable.Stop();

            if (IsLastInstance(lastPoint.Navigable))
            {
                _disposingInProgress = _stack.Count - 1;
                lastPoint.Navigable.LongDispose(() =>
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

                ProcessPushNew(name, parameters, dontReuseContext);
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

        private bool IsLastInstance(INavigable navigable)
        {
            for (int i = _stack.Count - 1; i > 0; i--) //skip last element, it's the one we're testing
            {
                if (_stack[i - 1].Navigable == navigable)
                    return false;
            }

            return true;
        }

        private INavigable GetLastContext(string name)
        {
            for (int i = _stack.Count; i > 0; i--)
            {
                if (_stack[i - 1].Name == name)
                    return _stack[i - 1].Navigable;
            }

            return null;
        }
    }
}