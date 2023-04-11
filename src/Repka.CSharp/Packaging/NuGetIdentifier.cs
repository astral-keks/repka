using Repka.Strings;

namespace Repka.Packaging
{
    public class NuGetIdentifier : Normalizable, IComparable<NuGetIdentifier>
    {
        private readonly string _value;

        public static implicit operator string(NuGetIdentifier identifier) => identifier._value;
        public static implicit operator NuGetIdentifier(string value) => new(value);
        public NuGetIdentifier(string value) : base(value)
        {
            _value = value;
        }

        public static bool operator ==(NuGetIdentifier? left, NuGetIdentifier? right) => Equals(left, right);

        public static bool operator !=(NuGetIdentifier? left, NuGetIdentifier? right) => !Equals(left, right);

        public int CompareTo(NuGetIdentifier? other)
        {
            return string.Compare(Normalized, other?.Normalized);
        }

        public override bool Equals(object? obj)
        {
            return obj is NuGetIdentifier identifier &&
                Equals(Normalized, identifier.Normalized);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Normalized);
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
