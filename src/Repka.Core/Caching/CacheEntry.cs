
namespace Repka.Caching
{
    public class CacheEntry
    {
        public CacheEntry(CacheEntry entry)
            : this(entry.Key, entry.Content, entry.Properties.ToList())
        {

        }

        public CacheEntry(string key, CacheContent content, List<CacheProperty> properties)
            : this(key, () => content, properties)
        { 
        }

        public CacheEntry(string key, Func<CacheContent> content, List<CacheProperty> properties)
        {
            Key = key;
            _content = new(content, true);
            Properties = properties;
        }

        public string Key { get; }

        private readonly Lazy<CacheContent> _content;
        public CacheContent Content => _content.Value;

        public IReadOnlyList<CacheProperty> Properties { get; }

        public class Builder
        {
            public string Key { get; set; } = string.Empty;

            public CacheContent Content { get; set; } = new();

            public List<CacheProperty> Properties { get; set; } = new();

            public CacheEntry Build() => new(Key, Content, Properties);
        }
    }
}
