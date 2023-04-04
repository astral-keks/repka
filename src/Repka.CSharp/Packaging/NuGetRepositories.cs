using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Repositories;

namespace Repka.Packaging
{
    internal static class NuGetRepositories
    {
        public static NuGetVersion? DiscoverPackageVersion(this SourceRepositoryProvider provider, NuGetIdentifier packageId, SourceCacheContext cacheContext)
        {
            return provider.GetRepositories().DiscoverPackageVersion(packageId, cacheContext);
        }

        public static NuGetVersion? DiscoverPackageVersion(this IEnumerable<SourceRepository> repositories, NuGetIdentifier packageId, SourceCacheContext cacheContext)
        {
            return Task.WhenAll(repositories.Select(repository => repository.DiscoverPackageVersionAsync(packageId, cacheContext)))
                .GetAwaiter().GetResult()
                .FirstOrDefault(version => version is not null);
        }

        public static async Task<NuGetVersion?> DiscoverPackageVersionAsync(this SourceRepository repository, NuGetIdentifier packageId, SourceCacheContext cacheContext)
        {
            FindPackageByIdResource packageResource = repository.GetResource<FindPackageByIdResource>();
            IEnumerable<NuGetVersion> packageVersions = await packageResource.GetAllVersionsAsync(packageId.ToString(), cacheContext, NullLogger.Instance, CancellationToken.None);
            return packageVersions.Max(version => version);
        }

        public static NuGetPackage? FindPackage(this NuGetv3LocalRepository localRepository, NuGetDescriptor packageDescriptor)
        {
            NuGetPackage? package = default;

            LocalPackageInfo localPackageInfo = localRepository.FindPackage(packageDescriptor.Id.ToString(), packageDescriptor.Version);
            if (localPackageInfo is not null)
                package = new NuGetPackage(localPackageInfo);

            return package;
        }

        public static bool DownloadPackage(this SourceRepositoryProvider provider, NuGetDescriptor packageDescriptor,
            VersionFolderPathResolver pathResolver, SourceCacheContext cacheContext, PackageExtractionContext extractionContext)
        {
            return provider.GetRepositories().DownloadPackage(packageDescriptor, pathResolver, cacheContext, extractionContext);
        }

        public static bool DownloadPackage(this IEnumerable<SourceRepository> repositories, NuGetDescriptor packageDescriptor,
            VersionFolderPathResolver pathResolver, SourceCacheContext cacheContext, PackageExtractionContext extractionContext)
        {
            return packageDescriptor.Version is not null
                ? Task.WhenAll(repositories.Select(repository => repository.DownloadPackageAsync(packageDescriptor, pathResolver, cacheContext, extractionContext)))
                    .GetAwaiter().GetResult()
                    .FirstOrDefault(success => success)
                : default;
        }

        public static async Task<bool> DownloadPackageAsync(this SourceRepository repository, NuGetDescriptor packageDescriptor,
            VersionFolderPathResolver pathResolver, SourceCacheContext cacheContext, PackageExtractionContext extractionContext)
        {
            bool result = false;

            if (packageDescriptor.Version is not null)
            {
                PackageIdentity package = new(packageDescriptor.Id.ToString(), packageDescriptor.Version);

                FindPackageByIdResource packageResource = repository.GetResource<FindPackageByIdResource>();
                IPackageDownloader? packageDownloader = await packageResource.GetPackageDownloaderAsync(package,
                    cacheContext, NullLogger.Instance, CancellationToken.None);
                if (packageDownloader is not null)
                {
                    result = await PackageExtractor.InstallFromSourceAsync(package, packageDownloader, pathResolver, extractionContext, CancellationToken.None);
                }
            }

            return result;
        }
    }
}
