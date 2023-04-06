namespace Repka.Graphs
{
    public abstract class GraphAttribute
    {
        private readonly GraphKey _key;
        private readonly Type _valueType;
        private readonly Lazy<object?> _valueLazy;

        public static GraphAttribute<TValue> Of<TValue>(GraphKey key, Func<TValue?> factory) => new(key, factory);

        protected GraphAttribute(GraphKey key, Type type, Func<object?> factory)
        {
            _key = key;
            _valueType = type;
            _valueLazy = new(factory);
        }

        public GraphKey Key => _key;

        public object? Value => _valueLazy.Value;

        public override bool Equals(object? obj)
        {
            return obj is GraphAttribute value &&
                   Equals(_key, value._key) &&
                   Equals(_valueType, value._valueType);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_valueType, _key);
        }
    }

    public class GraphAttribute<TValue> : GraphAttribute
    {
        public GraphAttribute(GraphKey key, Func<TValue?> factory)
            : base(key, typeof(TValue), () => factory())
        {
        }

        public new TValue? Value => (TValue?)base.Value;
    }
}
