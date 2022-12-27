using System.Collections.Concurrent;

namespace Repka.Caching
{
    public class CacheProvider
    {
        public Cache GetCache(string cacheName)
        {
            ConcurrentDictionary<string, CacheEntry> entries = new();
            foreach (var entry in Read(cacheName))
                entries[entry.Key] = entry;

            return new Cache(entries, entries => Write(cacheName, entries));
        }

        protected virtual List<CacheEntry> Read(string cacheName) => new(0);

        protected virtual void Write(string cacheName, IEnumerable<CacheEntry> entries) { }
    }
}
