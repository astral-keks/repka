using Repka.Paths;
using Repka.Strings;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Repka.Graphs
{
    public class GraphKey : Normalizable, IComparable<GraphKey>
    {
        private readonly string _value;

        public static readonly GraphKey Null = new("null");

        public static GraphKey Compose(params GraphKey[] keys)
        {
            return Compose(keys.Select(key => key._value).ToArray());
        }

        public static GraphKey Compose(params string[] values)
        {
            return Compose(values, 0) ?? throw new ArgumentException("Key could not be created");
        }

        private static GraphKey? Compose(string[] values, int index)
        {
            GraphKey? key = null;

            if (index < values.Length)
            { 
                string first = values[index];
                GraphKey? second = Compose(values, index + 1);
                key = second is not null
                    ? new GraphKey($"{first}+{second}")
                    : new GraphKey(first);
            }

            return key;
        }

        public static implicit operator string(GraphKey key) => key._value;
        public static implicit operator GraphKey(string value) => new(value);
        public GraphKey(string value) : base(value)
        {
            _value = value;
        }

        public bool ContainsAny(params string[] texts)
        {
            return texts.Any(Contains);
        }

        public bool Contains(string text)
        {
            return _value.Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        public bool Matches(string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.IsMatch(_value, pattern, options);
        }

        public static bool operator !=(GraphKey? first, GraphKey? second)
        {
            return !(first == second);
        }

        public static bool operator ==(GraphKey? first, GraphKey? second)
        {
            return ReferenceEquals(first, second) || first?.Equals(second) == true;
        }

        public int CompareTo(GraphKey? other)
        {
            return string.Compare(Normalized, other?.Normalized);
        }

        public override bool Equals(object? obj)
        {
            return obj is GraphKey key && 
                Equals(Normalized, key.Normalized);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Normalized);
        }

        public Guid AsGuid()
        {
            byte[] data = Encoding.UTF8.GetBytes(Normalized);

            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);

            return new Guid(hash);
        }

        public AbsolutePath AsAbsolutePath() => new(_value);

        public override string ToString()
        {
            return _value;
        }
    }
}
