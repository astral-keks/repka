using System.Collections.Concurrent;

namespace Repka.Caching
{
    public sealed class Cache : IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _entries;
        private readonly Action<ICollection<CacheEntry>> _flush;

        public Cache(ConcurrentDictionary<string, CacheEntry> entries, Action<IEnumerable<CacheEntry>> flush)
        {
            _entries = entries;
            _flush = flush;
        }

        public CacheEntry? GetOrAdd(CacheEntry entry)
        {
            CacheEntry? cached = Get(entry.Key);
            
            if (cached is not null)
            {
                for (int i = 0; i < entry.Properties.Count; i++)
                {
                    CacheProperty entryProperty = entry.Properties[i];
                    CacheProperty? cachedProperty = i < cached.Properties.Count
                        ? cached.Properties[i]
                        : default;
                    if (entryProperty.Equals(cachedProperty))
                    {
                        return cached;
                    }
                }
            }

            Add(entry);
            return null;
        }

        public CacheEntry? Get(string key)
        {
            return _entries.TryGetValue(key, out CacheEntry? entry) ? entry : default;
        }

        public void Add(CacheEntry entry)
        {
            _entries[entry.Key] = new(entry);
        }

        public void Flush()
        {
            _flush(_entries.Values);
        }

        public void Dispose()
        {
            Flush();
        }
    }
}
