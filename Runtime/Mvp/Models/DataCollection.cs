using System.Collections.Generic;
using JetBrains.Annotations;

namespace Behc.Mvp.Models
{
    public class DataCollection : ReactiveModel, IDataCollection
    {
        protected readonly List<object> _data = new List<object>();

        public IReadOnlyCollection<object> Data => _data;

        public void Add([NotNull] object item)
        {
            _data.Add(item);
            NotifyChanges();
        }

        public void AddRange([ItemNotNull] IEnumerable<object> items)
        {
            _data.AddRange(items);
            NotifyChanges();
        }

        public void Clear()
        {
            _data.Clear();
            NotifyChanges();
        }
    }

    public class DataCollection<T> : ReactiveModel, IDataCollection where T : class
    {
        protected readonly List<T> _data = new List<T>();

        public IReadOnlyList<T> Data => _data;

        IReadOnlyCollection<object> IDataCollection.Data => _data;

        public void Add([NotNull] T item)
        {
            _data.Add(item);
            NotifyChanges();
        }

        public void AddRange([ItemNotNull] IEnumerable<T> items)
        {
            _data.AddRange(items);
            NotifyChanges();
        }

        public void Clear()
        {
            _data.Clear();
            NotifyChanges();
        }
    }
}