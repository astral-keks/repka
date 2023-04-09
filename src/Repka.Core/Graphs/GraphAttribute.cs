namespace Repka.Graphs
{
    public class GraphAttribute
    {
        private Lazy<object?>? _provider;

        public GraphAttribute(string name, Func<object?>? factory = default)
        {
            Name = name;
            if (factory is not null)
                _provider = new(factory, true);
        }

        public string Name { get; }

        public TValue? GetValue<TValue>() => (TValue?)_provider?.Value;

        public void SetValue<TValue>(Func<TValue> factory) => _provider = new(() => factory(), true);

        public void SetValue<TValue>(TValue value) => SetValue(() => value);

        public TValue Value<TValue>(Func<TValue> factory)
        {
            if (_provider is null)
                _provider = new(() => factory(), true);
            return GetValue<TValue>()!;
        }

        public override bool Equals(object? obj)
        {
            return obj is GraphAttribute attribute &&
                   Equals(Name, attribute.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public override string ToString()
        {
            return $"Name={Name}";
        }
    }
}
