using Microsoft.CodeAnalysis;
using NuGet.Packaging;
using NuGet.Repositories;
using NuGet.Versioning;
using System.Text.RegularExpressions;

namespace Repka.Packaging
{
    public class NuGetPackage
    {
        private readonly string _location;
        private readonly bool _developmentDependency;
        private readonly IReadOnlyList<string> _files;
        private readonly IReadOnlyList<PackageDependencyGroup> _packageDependencies;
        private readonly IReadOnlyList<FrameworkSpecificGroup> _frameworkDependencies;

        public NuGetPackage(LocalPackageInfo packageInfo)
        {
            Id = new(packageInfo.Id);
            Version = packageInfo.Version;
            _location = packageInfo.ExpandedPath;
            _developmentDependency = packageInfo.Nuspec.GetDevelopmentDependency();
            _files = packageInfo.Files;
            _packageDependencies = packageInfo.Nuspec.GetDependencyGroups().ToList();
            _frameworkDependencies = packageInfo.Nuspec.GetFrameworkAssemblyGroups().ToList();
            _assemblies = new(GetAssemblies, true);
            _packageReferences = new(GetPackageReferences, true);
            _frameworkReferences = new(GetFrameworkReferences, true);
            _dllsWithTfm = new(GetDllsWithTfm, true);
        }

        public NuGetIdentifier Id { get; }

        public NuGetVersion Version { get; }

        public bool IsDevelopmentDependency => _developmentDependency;

        public IReadOnlyList<NuGetReference<NuGetDescriptor>> PackageReferences => _packageReferences.Value;
        private readonly Lazy<List<NuGetReference<NuGetDescriptor>>> _packageReferences;
        private List<NuGetReference<NuGetDescriptor>> GetPackageReferences()
        {
            return _packageDependencies
                .SelectMany(group => group.Packages.Any()
                    ? group.Packages.Select(package => NuGetReference.Of(group.TargetFramework, NuGetDescriptor.Of(package))).ToList()
                    : new() { NuGetReference.Of(group.TargetFramework, default(NuGetDescriptor)) })
                .ToList();
        }

        public IReadOnlyList<NuGetReference<string>> FrameworkReferences => _frameworkReferences.Value;
        private readonly Lazy<List<NuGetReference<string>>> _frameworkReferences;
        public List<NuGetReference<string>> GetFrameworkReferences()
        {
            return _frameworkDependencies
                .SelectMany(group => group.Items.Any()
                    ? group.Items.Select(item => NuGetReference.Of(group.TargetFramework, item)).ToList()
                    : new() { NuGetReference.Of(group.TargetFramework, default(string)) })
                .ToList();
        }

        public IReadOnlyList<NuGetReference<string>> Assemblies => _assemblies.Value;
        private readonly Lazy<List<NuGetReference<string>>> _assemblies;
        private List<NuGetReference<string>> GetAssemblies()
        {
            List<NuGetReference<string>> assemblies;
            if (DllsWithTfm.Any())
                assemblies = DllsWithTfm.Select(dll => NuGetReference.Of(dll.Tfm.Framework, dll.Path)).ToList();
            else
                assemblies = NuGetMonikers.AllFrameworks.SelectMany(framework => GetFiles(".dll").Select(dll => NuGetReference.Of(framework, dll))).ToList();

            return assemblies;
        }

        private IReadOnlyList<(NuGetMoniker Tfm, string? Path)> DllsWithTfm => _dllsWithTfm.Value;
        private readonly Lazy<List<(NuGetMoniker Tfm, string? Path)>> _dllsWithTfm;
        private static readonly Regex _libRegex = new(@"^lib[/\\]([^/\\]+)[/\\][^/\\]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _refRegex = new(@"^ref[/\\]([^/\\]+)[/\\][^/\\]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private List<(NuGetMoniker Tfm, string? Path)> GetDllsWithTfm()
        {
            List<(NuGetMoniker Tfm, string? Path)> dllsWithTfm = GetDlls(_libRegex);
            if (!dllsWithTfm.Any())
                dllsWithTfm = GetDlls(_refRegex);
            return dllsWithTfm;
        }

        private List<(NuGetMoniker Tfm, string? Path)> GetDlls(Regex regex)
        {
            return GetFiles()
                .Where(path => 
                    path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || 
                    path.EndsWith("_._", StringComparison.OrdinalIgnoreCase))
                .Select(path => regex.Match(path))
                .Where(match => match.Success)
                .Select(match => (Tfm: NuGetMoniker.Resolve(match.Groups[1].Value), Path: match.Value))
                .Where(dll => dll.Tfm is not null)
                .OfType<(NuGetMoniker Tfm, string? Path)>()
                .Select(dll =>
                {
                    if (dll.Path is not null)
                    {
                        dll = dll with
                        {
                            Path = Path.Combine(_location, dll.Path)
                                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                        };
                    }
                    return dll;
                })
                .ToList();
        }

        private IEnumerable<string> GetFiles(string? extension = default)
        {
            return _files
                .Where(path => extension is null || path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }

        public override bool Equals(object? obj)
        {
            return obj is NuGetPackage package &&
                   Id.Equals(package.Id) &&
                   EqualityComparer<NuGetVersion?>.Default.Equals(Version, package.Version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }

        public override string ToString()
        {
            return $"{Id}:{Version}";
        }
    }
}
