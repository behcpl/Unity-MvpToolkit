using System.Collections.Generic;
using Behc.Mvp.Model;

namespace Behc.Mvp.DataCollection
{
    public abstract class DataCollection : ReactiveModel, IDataCollection
    {
        public abstract int ItemsCount { get; }
        public abstract IEnumerable<object> Items { get; }
        public abstract object GetItemId(object item);
    }
}