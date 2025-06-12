using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

namespace Behc.Mvp.Models
{
	public class DataStack : ReactiveModel, IDataCollection
	{
		private struct ItemType
		{
			public object Model;
			public Action<object> OnClose;
			public object DefaultResultValue;
			public bool CanDefaultClose;
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

		public object TopLevelData => _stack.Count > 0 ? _stack[_stack.Count - 1].Model : null;
		public IReadOnlyCollection<object> Data => _collection;

		private readonly List<ItemType> _stack;
		private readonly Collection _collection;

		public DataStack()
		{
			_stack = new List<ItemType>(16);
			_collection = new Collection(_stack);
		}

		public void Push<TResult>(object model, Action<TResult> onClose, TResult defaultResult = default, bool canDefaultClose = true)
		{
#if DEBUG
			Assert.IsNotNull(model);
			foreach (ItemType item in _stack)
				Assert.IsFalse(item.Model == model, $"Model {model} already on stack!");
#endif
			void OnClose(object result) => onClose?.Invoke((TResult)result);
			_stack.Add(new ItemType { Model = model, OnClose = OnClose, DefaultResultValue = defaultResult, CanDefaultClose = canDefaultClose });
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

		public bool TryRemoveTopLevel()
		{
			if (_stack.Count == 0)
				return false;

			if (CanDefaultClose())
			{
				ItemType item = _stack[_stack.Count - 1];
				_stack.RemoveAt(_stack.Count - 1);
				item.OnClose(item.DefaultResultValue);
				NotifyChanges();
			}

			return true;
		}

		public bool CanDefaultClose()
		{
			if (_stack.Count == 0)
				return false;

			ItemType item = _stack[_stack.Count - 1];
			bool canDefaultClose = (item.Model as ICloseOptions)?.CanDefaultClose ?? true;
			return canDefaultClose && item.CanDefaultClose;
		}
	}
}
