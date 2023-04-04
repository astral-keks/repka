namespace Repka.Packaging
{
    public class NuGetIdentifier
    {
        private readonly string _value;

        public NuGetIdentifier(string value)
        {
            _value = value;
        }

        public static bool operator ==(NuGetIdentifier? left, NuGetIdentifier? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NuGetIdentifier? left, NuGetIdentifier? right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object? obj)
        {
            return obj is NuGetIdentifier identifier &&
                   _value.Equals(identifier._value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value.ToLower());
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
