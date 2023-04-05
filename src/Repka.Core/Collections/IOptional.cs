namespace Repka.Collections
{
    public interface IOptional<out T> : IEnumerable<T>
    {
        public bool HasValue { get; }

        public T Value { get; }

        public T? OrElseDefault()
        {
            return HasValue ? Value : default;
        }

        public IOptional<T> Filter(Func<T, bool> predicate)
        {
            return HasValue && predicate(Value) ? this : Optional.Empty<T>();
        }

        public IOptional<R> FlatMap<R>(Func<T, IOptional<R>> selector)
        {
            return HasValue ? selector(Value) : Optional.Empty<R>();
        }

        public IOptional<R> Map<R>(Func<T, R> selector)
        {
            return HasValue ? selector(Value).ToOptional() : Optional.Empty<R>();
        }
    }
}
