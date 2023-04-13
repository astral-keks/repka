namespace Repka.Strings
{
    public class Normalized : Normalizable
    {
        public Normalized(string value) : base(value)
        {
            Original = value;
        }

        public string Original { get; }

        public bool Contains(string pattern)
            => Normalized.Contains(pattern, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj)
        {
            return obj is Normalized normalized &&
                Equals(Normalized, normalized.Normalized);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Normalized);
        }

        public override string ToString()
        {
            return Original;
        }
    }

    public static class NormalizedExtensions
    {
        public static Normalized Normalize(this string source) =>
            new Normalized(source);
    }
}
