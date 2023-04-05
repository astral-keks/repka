using System.Collections;

namespace Repka.Collections
{
    public static class Optional
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
