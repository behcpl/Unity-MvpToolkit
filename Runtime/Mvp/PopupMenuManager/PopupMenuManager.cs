using System.Collections.Generic;
using Behc.Mvp.Model;
using UnityEngine.Assertions;

namespace Behc.Mvp.PopupMenuManager
{
    public class PopupMenuManager : ReactiveModel, IDataCollection
    {
        public IEnumerable<object> Items => _items;
        public int ItemsCount => _items.Count;

        private readonly List<object> _items = new List<object>(16);

        public void Add(object model)
        {
            Assert.IsNotNull(model);
            Assert.IsFalse(_items.IndexOf(model) >= 0);

            _items.Add(model);
            NotifyChanges();
        }

        public void Remove(object model)
        {
            Assert.IsNotNull(model);
            Assert.IsTrue(_items.IndexOf(model) >= 0);

            _items.Remove(model);
            NotifyChanges();
        }
        
        public void RemoveAfter(object model)
        {
            Assert.IsNotNull(model);
            Assert.IsTrue(_items.IndexOf(model) >= 0);

            int index = _items.IndexOf(model);
            if (index < _items.Count - 1)
            {
                _items.RemoveRange(index + 1, _items.Count - index - 1);
                NotifyChanges();              
            }
        }

        public void RemoveAll()
        {
            _items.Clear();
            NotifyChanges();
        }
    }
}