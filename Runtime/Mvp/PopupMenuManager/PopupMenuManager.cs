using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Behc.Mvp.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace Behc.Mvp.PopupMenuManager
{
    public class PopupMenuManager : ReactiveModel, IDataCollection
    {
        public enum Placement
        {
            PLACE_AT_CURSOR,
            RIGHT_OF_RECT,
            LEFT_OF_RECT,
            TOP_OF_RECT,
            BOTTOM_OF_RECT,
        }

        private class ItemType
        {
            public object Model;
            public Placement Placement;
            public Rect Rect;
        }

        private class Collection : IReadOnlyCollection<object>
        {
            private readonly List<ItemType> _stack;

            public int Count => _stack.Count;

            public Collection(List<ItemType> stack)
            {
                _stack = stack;
            }

            public IEnumerator<object> GetEnumerator() => _stack.Select(i => i.Model).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public IReadOnlyCollection<object> Data => _collection;

        private readonly List<ItemType> _items;
        private readonly Collection _collection;

        public PopupMenuManager()
        {
            _items = new List<ItemType>(16);
            _collection = new Collection(_items);
        }

        public void Add(object model)
        {
            Assert.IsNotNull(model);
            Assert.IsNull(_items.Find(i => i.Model == model));

            _items.Add(new ItemType { Model = model, Placement = Placement.PLACE_AT_CURSOR });
            NotifyChanges();
        }

        public void Add(object model, Placement placement, Rect rect)
        {
            Assert.IsNotNull(model);
            Assert.IsNull(_items.Find(i => i.Model == model));

            _items.Add(new ItemType { Model = model, Placement = placement, Rect = rect });
            NotifyChanges();
        }

        public void Remove(object model)
        {
            Assert.IsNotNull(model);
            Assert.IsNotNull(_items.Find(i => i.Model == model));

            _items.RemoveAll(i => i.Model == model);
            NotifyChanges();
        }

        public void RemoveAllAfter(object model)
        {
            Assert.IsNotNull(model);

            int index = _items.FindIndex(0, i => i.Model == model);
            if (index >= 0 && index < _items.Count - 1)
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

        public Placement GetItemPlacement(object model)
        {
            return _items.Find(i => i.Model == model)?.Placement ?? Placement.PLACE_AT_CURSOR;
        }

        public Rect GetItemRect(object model)
        {
            return _items.Find(i => i.Model == model)?.Rect ?? Rect.zero;
        }
    }
}