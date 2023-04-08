using System.Collections;

namespace Repka.Collections
{
    public static class Optionals
    {
        public static IOptional<T> ToOptional<T>(this T? value)
        {
            return Of(value);
        }

        public static IOptional<T> Of<T>(T? value)
        {
            return new Optional<T>(value);
        }

        public static IOptional<T> Empty<T>()
        {
            return new Optional<T>(default);
        }
    }

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
            return HasValue && predicate(Value) ? this : Optionals.Empty<T>();
        }

        public IOptional<R> FlatMap<R>(Func<T, IOptional<R>> selector)
        {
            return HasValue ? selector(Value) : Optionals.Empty<R>();
        }

        public IOptional<R> Map<R>(Func<T, R> selector)
        {
            return HasValue ? selector(Value).ToOptional() : Optionals.Empty<R>();
        }
    }

    internal class Optional<T> : IOptional<T>
    {
        private readonly T? _value;

        internal Optional(T? value)
        {
            _value = value;
        }

        public bool HasValue => _value is not null;

        public T Value => _value is not null ? _value : throw new InvalidOperationException("Option value is empty");

        public IEnumerator<T> GetEnumerator()
        {
            if (HasValue)
                yield return Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
