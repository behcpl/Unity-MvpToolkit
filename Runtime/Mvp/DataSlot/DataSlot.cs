using Behc.Mvp.Model;

namespace Behc.Mvp.DataSlot
{
    public class DataSlot : ReactiveModel, IDataSlot
    {
        private object _data;

        public object Data
        {
            get => _data;
            set
            {
                if (ReferenceEquals(_data, value))
                    return;

                _data = value;
                NotifyChanges();
            }
        }
    }

    public class DataSlot<T> : ReactiveModel, IDataSlot where T : class
    {
        private T _data;

        object IDataSlot.Data => _data;

        public T Data
        {
            get => _data;
            set
            {
                if (ReferenceEquals(_data, value))
                    return;

                _data = value;
                NotifyChanges();
            }
        }
    }
}