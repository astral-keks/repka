namespace Repka.Graphs
{
    public abstract class GraphAttribute
    {
        private readonly Type _type;
        private readonly GraphToken _owner;
        private readonly Lazy<object?> _lazy;

        protected GraphAttribute(Type type, GraphToken owner, Func<object?> factory)
        {
            _type = type;
            _owner = owner;
            _lazy = new(factory);
        }

        public GraphToken Owner => _owner;

        public object? Value => _lazy.Value;

        public override bool Equals(object? obj)
        {
            return obj is GraphAttribute value &&
                   EqualityComparer<Type>.Default.Equals(_type, value._type) &&
                   EqualityComparer<GraphToken>.Default.Equals(_owner, value._owner);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_type, _owner);
        }
    }

    public class GraphAttribute<TValue> : GraphAttribute
    {
        public GraphAttribute(GraphToken owner, TValue value)
            : this(owner, () => value)
        {
        }

        public GraphAttribute(GraphToken owner, Func<TValue?> factory)
            : base(typeof(TValue), owner, () => factory())
        {
        }

        public new TValue? Value => (TValue?)base.Value;
    }
}
