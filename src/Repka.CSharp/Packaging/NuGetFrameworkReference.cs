using NuGet.Frameworks;

namespace Repka.Packaging
{
    public class NuGetFrameworkReference : NuGetAsset
    {
        public NuGetFrameworkReference(string? assemblyName, NuGetFramework framework) 
            : base(framework)
        {
            AssemblyName = assemblyName;
        }

        public string? AssemblyName { get; }
    }
}
