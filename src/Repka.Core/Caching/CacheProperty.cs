namespace Repka.Caching
{
    public class CacheProperty : IEquatable<CacheProperty>
    {
        public CacheProperty(string name, string value)
            : this(name, () => value)
        {
        }

        public CacheProperty(string name, Func<string> factory)
        {
            Name = name;
            _lazy = new(factory, true);
        }

        public string Name { get; }

        private readonly Lazy<string> _lazy;
        public string Value => _lazy.Value;

        public bool Equals(CacheProperty? other)
        {
            return Name == other?.Name && Value == other?.Value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as CacheProperty);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Value);
        }
    }
}
