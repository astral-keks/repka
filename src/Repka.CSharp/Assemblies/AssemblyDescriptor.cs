using Repka.Strings;
using System.Reflection;

namespace Repka.Assemblies
{
    public class AssemblyDescriptor : Normalizable
    {
        public AssemblyDescriptor(string location) : base(location)
        {
            Location = location;

            _exists = new(GetExists, true);
            _name = new(GetName, true);
        }

        public string Location { get; }

        public bool Exists => _exists.Value;
        private readonly Lazy<bool> _exists;
        private bool GetExists()
        {
            return File.Exists(Location);
        }

        public AssemblyName? Name => _name.Value;
        private readonly Lazy<AssemblyName?> _name;
        private AssemblyName? GetName()
        {
            try
            {
                return Exists ? AssemblyName.GetAssemblyName(Location) : default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is AssemblyDescriptor reference &&
                   Normalized == reference.Normalized;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Normalized);
        }
    }
}
