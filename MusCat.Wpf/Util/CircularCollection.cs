using System.Collections.Generic;

namespace MusCat.Util
{
    class CircularCollection<T>
    {
        private readonly IList<T> _items;

        private int _current = 0;

        public CircularCollection(IList<T> items)
        {
            _items = items;
        }

        public T GetCurrent()
        {
            return _items[_current];
        }

        public T GetNext()
        {
            return _current < _items.Count - 1 ? _items[_current + 1] : _items[0];
        }

        public T GetPrev()
        {
            return _current > 0 ? _items[_current - 1] : _items[_items.Count - 1];
        }

        public void Next()
        {
            _current = _current < _items.Count - 1 ? _current + 1 : 0;
        }

        public void Prev()
        {
            _current = _current > 0 ? _current - 1 : _items.Count - 1;
        }
    }
}
