using NuGet.Versioning;
using System.Collections.Concurrent;

namespace Repka.Packaging
{
    public class NuGetContext
    {
        private readonly ConcurrentDictionary<NuGetDescriptor, Lazy<NuGetPackage?>> _packages = new();
        private readonly ConcurrentDictionary<NuGetIdentifier, Lazy<NuGetVersion?>> _versions = new();

        public NuGetPackage? AddIfMissing(NuGetDescriptor packageDescriptor, Func<NuGetPackage?> packageFactory)
        {
            NuGetPackage? package = default;

            if (_packages.TryAdd(packageDescriptor, new Lazy<NuGetPackage?>(packageFactory, true)))
                package = _packages[packageDescriptor].Value;

            return package;
        }

        public NuGetPackage? GetOrAdd(NuGetDescriptor packageDescriptor, Func<NuGetPackage?> packageFactory)
        {
            return _packages.GetOrAdd(packageDescriptor, new Lazy<NuGetPackage?>(packageFactory, true)).Value;
        }

        public NuGetVersion? GetOrAdd(NuGetDescriptor packageDescriptor, Func<NuGetVersion?> versionFactory)
        {
            return _versions.GetOrAdd(packageDescriptor.Id, new Lazy<NuGetVersion?>(versionFactory, true)).Value;
        }
    }
}
