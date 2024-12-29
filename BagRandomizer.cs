namespace Tetris
{
    public class BagRandomizer<T>
    {
        private readonly Random _random = new();
        private List<T> _items;
        private Queue<int> _bagIndexs;

        public BagRandomizer() : this(new List<T>()) { }
        public BagRandomizer(Array items) : this(items.Cast<T>().ToList()) { }
        public BagRandomizer(List<T> items)
        {
            _items = new(items);
            _bagIndexs = new(_items.Count);
        }

        public void Reset(List<T> items)
        {
            _items = new(items);
            _bagIndexs = new(_items.Count);
        }

        public T Next()
        {
            if (_items.Count == 0)
            {
                throw new InvalidOperationException("No items in the bag.");
            }

            if (_bagIndexs.Count == 0)
            {
                RefillBag();
            }
            return _items[_bagIndexs.Dequeue()];
        }

        public void RefillBag()
        {
            _bagIndexs.Clear();
            foreach (var i in Enumerable.Range(0, _items.Count).OrderBy(i => _random.Next()))
            {
                _bagIndexs.Enqueue(i);
            }
        }
    }
}
