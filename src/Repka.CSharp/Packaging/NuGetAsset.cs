using NuGet.Frameworks;

namespace Repka.Packaging
{
    public abstract class NuGetAsset
    {
        public NuGetAsset(NuGetFramework framework)
        {
            Framework = framework;
        }

        public NuGetFramework Framework { get; }
    }
}
