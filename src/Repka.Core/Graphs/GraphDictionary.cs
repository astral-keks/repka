using Repka.Optionals;
using System.Collections;

namespace Repka.Graphs
{
    internal class GraphDictionary<TKey, TItem> : IEnumerable<TItem>
        where TKey : notnull
        where TItem : notnull
    {
        private readonly Dictionary<TItem, TItem> _items;
        private readonly Dictionary<TKey, ISet<TItem>> _index;
        private readonly Func<TItem, IEnumerable<TKey>> _keys;

        public GraphDictionary(Func<TItem, TKey> key)
            : this(item => key(item).ToOptional())
        {
        }

        public GraphDictionary(Func<TItem, IEnumerable<TKey>> keys)
        {
            _items = new();
            _index = new();
            _keys = keys;
        }

        public void Add(TItem item)
        {
            if (!_items.ContainsKey(item))
                _items.Add(item, item);

            foreach (var key in _keys(item))
            {
                if (!_index.ContainsKey(key))
                    _index.Add(key, new HashSet<TItem>(1));
                _index[key].Add(item);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _index.ContainsKey(key);
        }

        public bool Contains(TItem item)
        {
            return _items.ContainsKey(item);
        }

        public TItem? Find(TItem item)
        {
            return _items.ContainsKey(item) ? _items[item] : default;
        }

        public ISet<TItem> FindAll(TKey key)
        {
            return _index.ContainsKey(key) ? _index[key] : new HashSet<TItem>(0);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _index.Values.SelectMany(items => items).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
