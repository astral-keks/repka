using NuGet.Frameworks;
using System.Reflection;

namespace Repka.Packaging
{
    public class NuGetAssembly : NuGetAsset
    {
        public NuGetAssembly(string? locaton, NuGetFramework framework) 
            : base(framework)
        {
            Locaton = locaton;

            _name = new(GetName, true);
        }

        public string? Locaton { get; }

        public AssemblyName? Name => _name.Value;
        private readonly Lazy<AssemblyName?> _name;
        private AssemblyName? GetName()
        {
            try
            {
                return File.Exists(Locaton) ? AssemblyName.GetAssemblyName(Locaton) : default;
            }
            catch (Exception)
            {
                return default;
            }
        }

    }
}
