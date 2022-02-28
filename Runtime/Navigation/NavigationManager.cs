using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Behc.Navigation
{
    public class NavigationManager
    {
        private struct NavigationPoint
        {
            public string Name;
            public object Context;
        }

        private readonly NavigationRegistry _navigationRegistry;
        private readonly Stack<NavigationPoint> _stack;

        private NavigationPoint _current;
        private NavigationPoint _next;
        private bool _pushCurrent;

        public NavigationManager(NavigationRegistry navigationRegistry)
        {
            _navigationRegistry = navigationRegistry;
            _stack = new Stack<NavigationPoint>();
        }

        public void SetNavigationPoint(string name, object context, bool pushCurrent)
        {
            if (pushCurrent && !string.IsNullOrEmpty(_current.Name))
            {
                INavigable navigable = _navigationRegistry.Get(_current.Name);
                object currentContext = navigable.ValidateContext(_current.Context);
   
                _stack.Push(new NavigationPoint { Name = _current.Name, Context = currentContext});
            }

            _current = new NavigationPoint { Name = name, Context = context };
        }

        public bool GoBack()
        {
            if (_stack.Count <= 0)
                return false;
            
            _next = _stack.Pop();
            _pushCurrent = false;
            return true;
        }

        public void NavigateTo(string name, object context)
        {
            Assert.IsNotNull(_navigationRegistry.Get(name), $"Feature '{name}' not found!");

            _next = new NavigationPoint { Name = name, Context = context };
            _pushCurrent = true;
        }

        public void CommitDeferredChanges()
        {
            if (_next.Name == _current.Name && _next.Context == _current.Context) 
                return;

            string oldName = _current.Name;
            SetNavigationPoint(_next.Name, _next.Context, _pushCurrent);
            
            INavigable navigable = _navigationRegistry.Get(_current.Name);
            navigable.StartUp(_current.Context, oldName);
        }
    }
}