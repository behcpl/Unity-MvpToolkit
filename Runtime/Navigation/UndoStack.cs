using System.Collections.Generic;

namespace Behc.Navigation
{
    public class UndoStack
    {
        private struct NavigationPoint
        {
            public string Name;
            public object Context;
        }

        private readonly Stack<NavigationPoint> _undoStack = new Stack<NavigationPoint>();
        private readonly Stack<NavigationPoint> _redoStack = new Stack<NavigationPoint>();

        private string _currentName;
        private object _currentContext;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void SetCurrentPoint(string name, object context)
        {
            _currentName = name;
            _currentContext = context;
        }

        public void Push()
        {
            _undoStack.Push(new NavigationPoint { Name = _currentName, Context = _currentContext });
            _redoStack.Clear();
        }
        
        public void Undo(out string name, out object context)
        {
            NavigationPoint np = _undoStack.Pop();
            name = np.Name;
            context = np.Context;

            _redoStack.Push(np);
        }

        public void Redo(out string name, out object context)
        {
            NavigationPoint np = _redoStack.Pop();
            name = np.Name;
            context = np.Context;

            _undoStack.Push(np);
        }

        public void Reset()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}