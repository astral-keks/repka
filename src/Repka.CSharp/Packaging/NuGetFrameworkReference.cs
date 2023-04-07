using NuGet.Frameworks;

namespace Repka.Packaging
{
    public class NuGetFrameworkReference : NuGetAsset
    {
        public NuGetFrameworkReference(string? name, NuGetFramework framework) 
            : base(framework)
        {
            Name = name;
        }

        public string? Name { get; }
    }
}
