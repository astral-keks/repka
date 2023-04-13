using NuGet.Frameworks;

namespace Repka.Packaging
{
    public class NuGetAssemblyReference : NuGetAsset
    {
        public NuGetAssemblyReference(string? assemblyName, NuGetFramework framework) 
            : base(framework)
        {
            AssemblyName = assemblyName;
        }

        public string? AssemblyName { get; }
    }
}
