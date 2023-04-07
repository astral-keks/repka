using NuGet.Frameworks;

namespace Repka.Packaging
{
    public class NuGetPackageReference : NuGetAsset
    {
        public NuGetPackageReference(NuGetDescriptor? descriptor, NuGetFramework framework) 
            : base(framework)
        {
            Descriptor = descriptor;
        }

        public NuGetDescriptor? Descriptor { get; }
    }
}
