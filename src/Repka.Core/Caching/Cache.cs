using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Caching
{
    public abstract class Cache
    {
        public TValue GetOrAdd<TValue>(string key, DateTime date, Func<TValue> factory)
        {
            return GetOrAdd(new CacheEntry<TValue>(key, date, factory));
        }

        public abstract TValue GetOrAdd<TValue>(CacheEntry<TValue> entry);
    }
}
