using Repka.Paths;
using Repka.Strings;

namespace Repka.Assemblies
{
    public class AssemblyMetadata : Normalizable
    {
        public AssemblyMetadata(string location) : base(location)
        {
            Location = new(location);

            _exists = new(GetExists, true);
            _name = new(GetName, true);
        }

        public AbsolutePath Location { get; }

        public bool Exists => _exists.Value;
        private readonly Lazy<bool> _exists;
        private bool GetExists()
        {
            return File.Exists(Location);
        }

        public string? Name => _name.Value?.Name;

        public Version? Version => _name.Value?.Version;

        private readonly Lazy<System.Reflection.AssemblyName?> _name;
        private System.Reflection.AssemblyName? GetName()
        {
            try
            {
                return Exists ? System.Reflection.AssemblyName.GetAssemblyName(Location) : default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is AssemblyMetadata reference &&
                Equals(Normalized, reference.Normalized);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Normalized);
        }

        public override string ToString()
        {
            return Location;
        }
    }
}
