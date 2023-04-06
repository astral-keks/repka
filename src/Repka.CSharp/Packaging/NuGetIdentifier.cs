namespace Repka.Packaging
{
    public class NuGetIdentifier : IComparable<NuGetIdentifier>
    {
        private readonly string _value;

        public static implicit operator string(NuGetIdentifier identifier) => identifier._value;
        public static implicit operator NuGetIdentifier(string value) => new(value);
        public NuGetIdentifier(string value)
        {
            _value = value;
        }

        public static bool operator ==(NuGetIdentifier? left, NuGetIdentifier? right) => Equals(left, right);

        public static bool operator !=(NuGetIdentifier? left, NuGetIdentifier? right) => !Equals(left, right);

        public override bool Equals(object? obj)
        {
            return obj is NuGetIdentifier identifier &&
                   _value.Equals(identifier._value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value.ToLower());
        }

        public int CompareTo(NuGetIdentifier? other)
        {
            return string.Compare(_value, other?._value, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
