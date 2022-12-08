namespace Repka.Graphs
{
    public abstract class GraphAttribute
    {
        private readonly Type _type;
        private readonly GraphToken _referer;
        private readonly Lazy<object?> _lazy;

        protected GraphAttribute(Type type, GraphToken referer, Func<object?> factory)
        {
            _type = type;
            _referer = referer;
            _lazy = new(factory);
        }

        public GraphToken Referer => _referer;

        public object? Value => _lazy.Value;

        public override bool Equals(object? obj)
        {
            return obj is GraphAttribute value &&
                   EqualityComparer<Type>.Default.Equals(_type, value._type) &&
                   EqualityComparer<GraphToken>.Default.Equals(_referer, value._referer);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_type, _referer);
        }
    }

    public class GraphAttribute<TValue> : GraphAttribute
    {
        public GraphAttribute(GraphToken referer, TValue value)
            : this(referer, () => value)
        {
        }

        public GraphAttribute(GraphToken referer, Func<TValue?> factory)
            : base(typeof(TValue), referer, () => factory())
        {
        }

        public new TValue? Value => (TValue?)base.Value;
    }
}
