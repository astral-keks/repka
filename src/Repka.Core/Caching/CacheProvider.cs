using System.Collections.Concurrent;

namespace Repka.Caching
{
    public class CacheProvider
    {
        public Cache GetCache(string store, string name)
        {
            ConcurrentDictionary<string, CacheEntry> entries = new();
            foreach (var entry in Read(store, name))
                entries[entry.Key] = entry;

            return new Cache(entries, entries => Write(store, name, entries));
        }

        protected virtual List<CacheEntry> Read(string store, string name) => new(0);

        protected virtual void Write(string store, string name, IEnumerable<CacheEntry> entries) { }
    }
}
