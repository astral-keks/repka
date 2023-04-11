using NuGet.Frameworks;
using NuGet.Packaging.Core;

namespace Repka.Packaging
{
    public class NuGetPackageReference : NuGetAsset
    {
        public NuGetPackageReference(PackageDependency package, NuGetFramework framework)
            : this(NuGetDescriptor.Of(package), framework)
        {
        }

        public NuGetPackageReference(NuGetDescriptor? descriptor, NuGetFramework framework) 
            : base(framework)
        {
            Descriptor = descriptor;
        }

        public NuGetDescriptor? Descriptor { get; }
    }
}
