using Microsoft.Build.Construction;
using NuGet.Versioning;
using Repka.Projects;

namespace Repka.Packaging
{
    internal class NuGetOverrides
    {
        private readonly ProjectRootElement? _buildTargets;

        public static NuGetOverrides Load(DirectoryInfo? directory)
        {
            NuGetOverrides? overrides = default;

            if (directory is not null)
            {
                FileInfo buildTargetsFile = new FileInfo(Path.Combine(directory.FullName, "Directory.Build.targets"));
                if (buildTargetsFile?.Exists == true)
                {
                    ProjectRootElement buildTargets = buildTargetsFile.ToProject();
                    overrides = new(buildTargets);
                }
                else
                    overrides = Load(directory?.Parent);
            }

            return overrides ?? new();
        }

        public NuGetOverrides(ProjectRootElement? buildTargets = default)
        {
            _buildTargets = buildTargets;
            _packageVersions = new(GetPackageVersions, true);
        }

        public NuGetVersion? GetPackageVersion(NuGetIdentifier packageId) => _packageVersions.Value.ContainsKey(packageId)
            ? _packageVersions.Value[packageId]
            : default;
        private readonly Lazy<Dictionary<NuGetIdentifier, NuGetVersion?>> _packageVersions;
        private Dictionary<NuGetIdentifier, NuGetVersion?> GetPackageVersions()
        {
            return _buildTargets?.GetPackageReferences()
                .ToDictionary(
                    packageRef => new NuGetIdentifier(packageRef.Id),
                    packageRef => NuGetVersion.TryParse(packageRef.Version, out NuGetVersion? packageVersion)
                        ? packageVersion
                        : default)
                ?? new(0);
        }
    }
}
