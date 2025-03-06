using System;
using System.Collections.Generic;
using Behc.Utils;
using JetBrains.Annotations;

namespace Behc.Navigation
{
    public enum NavigationMode
    {
        REPLACE_LAST,
        REPLACE_ALL,
        KEEP_ALL
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
        private NavigationMode _nextMode;
        private bool _nextReuse;
        private object _nextParameters;
        private int _disposingInProgress;
        private Action _continuation;

        public NavigationManager(IFactory<string, object, INavigable> navigableFactory)
        {
            _navigableFactory = navigableFactory;
            _stack = new List<NavigationPoint>(16);
            _disposingInProgress = -1;
        }

        public void NavigateTo([NotNull] string name, [CanBeNull] object parameters, NavigationMode mode = NavigationMode.REPLACE_LAST, bool reuseContext = true)
        {
            ResetDeferredActions();
            _next = name;
            _nextMode = mode;
            _nextReuse = reuseContext;
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

        public bool TryGetActiveNavigationPoint(out string name, out INavigable navigable)
        {
            if (_stack.Count > 0)
            {
                int lastIndex = _stack.Count - 1;
                name = _stack[lastIndex].Name;
                navigable = _stack[lastIndex].Navigable;
                return true;
            }
            
            name = null;
            navigable = null;
            return false;
        }
        
        public IEnumerable<(string, INavigable)> GetNavigationPoints()
        {
            for (int index = _stack.Count - 1; index >= 0; index--)
            {
                NavigationPoint navPoint = _stack[index];
                yield return (navPoint.Name, navPoint.Navigable);
            }
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
                NavigationMode nextMode = _nextMode;
                bool nextReuse = _nextReuse;
                string next = _next;
                object parameters = _nextParameters;

                ResetDeferredActions();

                if (nextMode == NavigationMode.KEEP_ALL || _stack.Count == 0)
                {
                    ProcessPushNew(next, parameters, nextReuse);
                }
                else if (nextMode == NavigationMode.REPLACE_ALL)
                {
                    ProcessReplaceAll(next, parameters, nextReuse);
                }
                else
                {
                    ProcessReplaceCurrent(next, parameters, nextReuse);
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

        private void ProcessPushNew(string name, object parameters, bool reuseContext)
        {
            INavigable navigable = null;
            if (reuseContext)
            {
                navigable = GetLastContext(name);
            }

            navigable ??= _navigableFactory.Create(name, parameters);
            navigable.Start();

            _stack.Add(new NavigationPoint { Name = name, Navigable = navigable });
        }

        private void ProcessReplaceAll(string name, object parameters, bool reuseContext)
        {
            NavigationPoint lastPoint = _stack[_stack.Count - 1];
            if (lastPoint.Name != name || !reuseContext)
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
                    ProcessPushNew(name, parameters, reuseContext);
                }
            }
        }

        private void ProcessReplaceCurrent(string name, object parameters, bool reuseContext)
        {
            NavigationPoint lastPoint = _stack[_stack.Count - 1];

            if (lastPoint.Name == name && reuseContext)
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

                ProcessPushNew(name, parameters, reuseContext);
                _continuation = null;
            }
        }

        private void ResetDeferredActions()
        {
            _goBack = false;
            _next = null;
            _nextMode = NavigationMode.REPLACE_LAST;
            _nextReuse = true;
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