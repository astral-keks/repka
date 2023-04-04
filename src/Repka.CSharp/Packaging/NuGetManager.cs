using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;
using NuGet.Versioning;

namespace Repka.Packaging
{
    public class NuGetManager
    {
        private readonly NuGetv3LocalRepository _localRepository;
        private readonly SourceRepositoryProvider _repositoryProvider;
        private readonly VersionFolderPathResolver _pathResolver;
        private readonly NuGetOverrides _packageOverrides;
        private readonly SourceCacheContext _cacheContext;
        private readonly PackageExtractionContext _extractionContext;
        private readonly NuGetContext _directoryContext;

        internal NuGetManager(NuGetv3LocalRepository localRepository, SourceRepositoryProvider repositoryProvider, 
            NuGetOverrides packageOverrides, VersionFolderPathResolver pathResolver, 
            SourceCacheContext cacheContext, PackageExtractionContext extractionContext, 
            NuGetContext directoryContext)
        {
            _localRepository = localRepository;
            _repositoryProvider = repositoryProvider;
            _cacheContext = cacheContext;
            _pathResolver = pathResolver;
            _extractionContext = extractionContext;
            _directoryContext = directoryContext;
            _packageOverrides = packageOverrides;
        }

        public NuGetDescriptor ResolvePackage(NuGetDescriptor packageDescriptor)
        {
            NuGetVersion? packageVersion = _packageOverrides.GetPackageVersion(packageDescriptor.Id);
            if (packageVersion is not null)
                packageDescriptor = new(packageDescriptor.Id,  packageVersion);
            return DiscoverPackage(packageDescriptor);
        }

        public NuGetDescriptor DiscoverPackage(NuGetDescriptor packageDescriptor)
        {
            if (packageDescriptor.Version is null)
            {
                NuGetVersion? packageVersion = _directoryContext.GetOrAdd(packageDescriptor,
                    () => _repositoryProvider.DiscoverPackageVersion(packageDescriptor.Id, _cacheContext));
                packageDescriptor = new(packageDescriptor.Id, packageVersion);
            }

            return packageDescriptor;
        }

        public NuGetPackage? RestorePackage(NuGetDescriptor packageDescriptor)
        {
            packageDescriptor = DiscoverPackage(packageDescriptor);
            return _directoryContext.GetOrAdd(packageDescriptor, () =>
            {
                NuGetPackage? package = default;

                if (packageDescriptor.Version is not null)
                {
                    package = _localRepository.FindPackage(packageDescriptor);
                    if (package is null)
                    {
                        bool success = _repositoryProvider.DownloadPackage(packageDescriptor, _pathResolver, _cacheContext, _extractionContext);
                        if (success)
                            package = _localRepository.FindPackage(packageDescriptor);
                    }
                }

                if (package?.IsDevelopmentDependency == true)
                    package = default;

                return package;
            })  ;
        }
    }
}
