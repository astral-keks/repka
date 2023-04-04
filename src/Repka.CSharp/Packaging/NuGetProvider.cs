using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using NuGet.Protocol.Core.Types;
using NuGet.Repositories;

namespace Repka.Packaging
{
    public class NuGetProvider
    {
        public string? Root { get; init; }

        private NuGetContext Context { get; } = new();

        public NuGetManager GetManager(string path)
        {
            ISettings settings = Settings.LoadDefaultSettings(Root ?? path);
            string packagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);

            NuGetv3LocalRepository localRepository = new(packagesFolder);

            PackageSourceProvider sourceProvider = new(settings);
            SourceRepositoryProvider repositoryProvider = new(sourceProvider, Repository.Provider.GetCoreV3());

            SourceCacheContext cacheContext = new();
            VersionFolderPathResolver pathResolver = new(packagesFolder);
            NuGetOverrides packageOverrides = NuGetOverrides.Load(new DirectoryInfo(Root ?? path));
            ClientPolicyContext clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, NullLogger.Instance);
            PackageExtractionContext extractionContext = new(PackageSaveMode.Defaultv3, XmlDocFileSaveMode.Skip, clientPolicyContext, NullLogger.Instance);

            return new NuGetManager(localRepository, repositoryProvider, packageOverrides, pathResolver, cacheContext, extractionContext, Context);
        }
    }
}
