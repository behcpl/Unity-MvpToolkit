using System.Collections.Generic;

namespace Behc.Mvp.Utils
{
    public struct SmallSet //: ISet<object> - lots on non-essential stuff here
    {
        private object _item;
        private HashSet<object> _set; //for small items count List<> might be enough?

        public int Count
        {
            get
            {
                if (_set != null)
                    return _set.Count;

                return _item != null ? 1 : 0;
            }
        }

        public bool Add(object item)
        {
            if (item == null)
                return false;

            if (_set != null)
                return _set.Add(item);

            if (_item == item)
                return false;

            if (_item == null)
            {
                _item = item;
                return true;
            }

            _set = new HashSet<object> { _item, item };
            _item = null;

            return true;
        }

        public bool Remove(object item)
        {
            if (item == null)
                return false;

            if (_set != null)
                return _set.Remove(item);

            if (_item == item)
            {
                _item = null;
                return true;
            }

            return false;
        }

        public bool Contains(object item)
        {
            if (item == null)
                return false;

            if (_set != null)
                return _set.Contains(item);

            return _item == item;
        }

        public void Clear()
        {
            _set?.Clear();
            _item = null;
        }
    }
}