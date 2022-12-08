using Repka.Optionals;
using System.Collections;

namespace Repka.Graphs
{
    internal class GraphDictionary<TKey, TItem> : IEnumerable<TItem>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, ISet<TItem>> _items;
        private readonly Func<TItem, IEnumerable<TKey>> _keys;

        public GraphDictionary(Func<TItem, TKey> key)
            : this(item => key(item).ToOptional())
        {
        }

        public GraphDictionary(Func<TItem, IEnumerable<TKey>> keys)
        {
            _items = new();
            _keys = keys;
        }

        public void Add(TItem item, Action<TItem>? callback = null)
        {
            foreach (var key in _keys(item))
            {
                if (!_items.ContainsKey(key))
                    _items.Add(key, new HashSet<TItem>(1));
                if (!_items[key].Add(item) && callback is not null)
                    callback(_items[key].First(i => Equals(i, item)));
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _items.ContainsKey(key);
        }

        public bool Contains(TItem item)
        {
            return _keys(item).Any(_items.ContainsKey);
        }

        public ISet<TItem> Get(TKey key)
        {
            return _items.ContainsKey(key) ? _items[key] : new HashSet<TItem>(0);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _items.Values.SelectMany(items => items).Distinct().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
