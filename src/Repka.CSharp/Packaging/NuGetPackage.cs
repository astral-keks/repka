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

        public IReadOnlyList<NuGetPackageReference> PackageReferences => _packageReferences.Value;
        private readonly Lazy<List<NuGetPackageReference>> _packageReferences;
        private List<NuGetPackageReference> GetPackageReferences()
        {
            return _packageDependencies
                .SelectMany(group => group.Packages.Any()
                    ? group.Packages.Select(package => new NuGetPackageReference(NuGetDescriptor.Of(package), group.TargetFramework)).ToList()
                    : new() { new NuGetPackageReference(default, group.TargetFramework) })
                .ToList();
        }

        public IReadOnlyList<NuGetFrameworkReference> FrameworkReferences => _frameworkReferences.Value;
        private readonly Lazy<List<NuGetFrameworkReference>> _frameworkReferences;
        public List<NuGetFrameworkReference> GetFrameworkReferences()
        {
            return _frameworkDependencies
                .SelectMany(group => group.Items.Any()
                    ? group.Items.Select(item => new NuGetFrameworkReference(item, group.TargetFramework)).ToList()
                    : new() { new NuGetFrameworkReference(default, group.TargetFramework) })
                .ToList();
        }

        public IReadOnlyList<NuGetAssembly> Assemblies => _assemblies.Value;
        private readonly Lazy<List<NuGetAssembly>> _assemblies;
        private List<NuGetAssembly> GetAssemblies()
        {
            List<NuGetAssembly> assemblies;
            if (DllsWithTfm.Any())
                assemblies = DllsWithTfm
                    .Select(dll => new NuGetAssembly(ResolveFile(dll.Path), dll.Tfm.Framework))
                    .ToList();
            else
                assemblies = NuGetMonikers.AllFrameworks
                    .SelectMany(framework => GetFiles(".dll")
                    .Select(dll => new NuGetAssembly(ResolveFile(dll), framework)))
                    .ToList();

            return assemblies;
        }

        private IReadOnlyList<(NuGetMoniker Tfm, string Path)> DllsWithTfm => _dllsWithTfm.Value;
        private readonly Lazy<List<(NuGetMoniker Tfm, string Path)>> _dllsWithTfm;
        private static readonly Regex _libRegex = new(@"^lib[/\\]([^/\\]+)[/\\][^/\\]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _refRegex = new(@"^ref[/\\]([^/\\]+)[/\\][^/\\]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private List<(NuGetMoniker Tfm, string Path)> GetDllsWithTfm()
        {
            List<(NuGetMoniker Tfm, string Path)> dllsWithTfm = GetDlls(_libRegex).ToList();
            if (!dllsWithTfm.Any())
                dllsWithTfm = GetDlls(_refRegex).ToList();
            return dllsWithTfm;
        }

        private IEnumerable<(NuGetMoniker Tfm, string Path)> GetDlls(Regex regex)
        {
            return GetFiles()
                .Where(path => 
                    path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || 
                    path.EndsWith("_._", StringComparison.OrdinalIgnoreCase))
                .Select(path => regex.Match(path))
                .Where(match => match.Success)
                .Select(match => (Tfm: NuGetMoniker.Resolve(match.Groups[1].Value), Path: match.Value))
                .Where(dll => dll.Tfm is not null)
                .OfType<(NuGetMoniker Tfm, string Path)>();
        }

        private IEnumerable<string> GetFiles(string? extension = default)
        {
            return _files.Where(path => extension is null || path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }

        private string? ResolveFile(string? path)
        {
            if (path is not null)
                path = Path.Combine(_location, path).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return path;
        }

        public override bool Equals(object? obj)
        {
            return obj is NuGetPackage package &&
                   Id == package.Id &&
                   Version == package.Version;
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
