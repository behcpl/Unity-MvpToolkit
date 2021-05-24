using System;
using System.Collections.Generic;
using System.Linq;
using Behc.Mvp.Model;
using UnityEngine.Assertions;

namespace Behc.Mvp.DataStack
{
    public class DataStack : ReactiveModel, IDataCollection
    {
        private struct ItemType
        {
            public object Model;
            public Action<object> OnClose;
            public object DefaultResultValue;
        }

        public object TopLevelData => _stack.Count > 0 ? _stack[_stack.Count - 1].Model : null;
        public IEnumerable<object> Items => _stack.Select(i => i.Model);
        public int ItemsCount => _stack.Count;

        private readonly List<ItemType> _stack = new List<ItemType>(16);

        public void Push<TResult>(object model, Action<TResult> onClose, TResult defaultResult = default)
        {
#if DEBUG
            Assert.IsNotNull(model);
            foreach (ItemType item in _stack)
                Assert.IsFalse(item.Model == model, $"Model {model} already on stack!");
#endif
            void OnClose(object result) => onClose?.Invoke((TResult) result);
            _stack.Add(new ItemType { Model = model, OnClose = OnClose, DefaultResultValue = defaultResult });
            NotifyChanges();
        }
        
        public void Remove(object model)
        {
            int index = _stack.FindIndex(i => i.Model == model);
            //TODO: expect last item, should assert that? allow out of order window closing?
            Assert.IsTrue(index >= 0, $"Model {model} not found on stack!");

            ItemType item = _stack[index];
            _stack.RemoveAt(index);

            item.OnClose(item.DefaultResultValue);

            NotifyChanges();
        }

        public void Remove(object model, object result)
        {
            int index = _stack.FindIndex(i => i.Model == model);
            //TODO: expect last item, should assert that? allow out of order window closing?
            Assert.IsTrue(index >= 0, $"Model {model} not found on stack!");

            ItemType item = _stack[index];
            _stack.RemoveAt(index);

            item.OnClose(result);

            NotifyChanges();
        }

        public void RemoveAll()
        {
            if (_stack.Count == 0)
                return;

            ItemType[] copy = _stack.ToArray();
            _stack.Clear();

            for (int i = copy.Length - 1; i >= 0; i--)
            {
                copy[i].OnClose(copy[i].DefaultResultValue);
            }

            NotifyChanges();
        }
    }
}