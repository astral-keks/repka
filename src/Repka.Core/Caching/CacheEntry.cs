using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Caching
{
    public class CacheEntry<TValue>
    {
        private readonly Lazy<TValue> _lazy;

        public CacheEntry(string key, DateTime date, TValue value)
            : this(key, date, () => value)
        {
        }

        public CacheEntry(string key, DateTime date, Func<TValue> factory)
        {
            Key = key;
            Date = date;
            _lazy = new(factory);
        }

        public string Key { get; }

        public DateTime Date { get; }

        public TValue Value => _lazy.Value;
    }
}
