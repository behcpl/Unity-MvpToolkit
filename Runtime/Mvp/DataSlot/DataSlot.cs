using Behc.Mvp.Model;

namespace Behc.Mvp.DataSlot
{
    public class DataSlot : ReactiveModel
    {
        private object _data;

        public object Data
        {
            get => _data;
            set
            {
                _data = value;
                NotifyChanges();
            }
        }
    }
}