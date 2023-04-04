using NuGet.Versioning;
using Repka.Assemblies;
using Repka.Packaging;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class PackageDsl
    {
        public static PackageNode? Package(this Graph graph, GraphKey key) => graph.Node(key).AsPackage();

        public static IEnumerable<PackageNode> Packages(this Graph graph) => graph.Nodes()
            .Select(node => node.AsPackage()).OfType<PackageNode>();

        public static PackageNode? AsPackage(this GraphNode? node) => node?.Labels.Contains(PackageLabels.Package) == true
            ? new PackageNode(node)
            : default;

        public class PackageNode : GraphNode
        {
            private readonly PackageKey _key;
            private readonly NuGetCompatibility _assembliesCompatibility;
            private readonly NuGetCompatibility _frameworkDependenciesCompatibility;
            private readonly NuGetCompatibility _packageDependenciesCompatibility;

            internal PackageNode(GraphNode node) : base(node)
            {
                _key = PackageKey.Parse(Key);
                _assembliesCompatibility = GetCompatibility(PackageLabels.PackageAssembly);
                _frameworkDependenciesCompatibility = GetCompatibility(PackageLabels.FrameworkDependency);
                _packageDependenciesCompatibility = GetCompatibility(PackageLabels.PackageDependency);
            }

            public NuGetIdentifier Id => new(_key.Id);

            public NuGetVersion? Version => NuGetVersion.TryParse(_key.Version, out NuGetVersion? nugetVersion) 
                ? nugetVersion 
                : default;

            public ProjectNode? Project => Inputs(ProjectLabels.PackageDefinition)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>()
                .FirstOrDefault();

            public IEnumerable<ProjectNode> ReferencingProjects => Inputs(ProjectLabels.ProjectReference)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();

            public IEnumerable<AssemblyFile> PackageAssemblies(string? targetFramework) => _assembliesCompatibility.Resolve(targetFramework)
                .SelectMany(tfm => Outputs(PackageLabels.PackageAssembly, tfm))
                .Select(link => new AssemblyFile(link.TargetKey));

            public IEnumerable<AssemblyFile> FrameworkDependencies(string? targetFramework) => _frameworkDependenciesCompatibility.Resolve(targetFramework)
                .SelectMany(tfm => Outputs(PackageLabels.FrameworkDependency, tfm))
                .Select(link => new AssemblyFile(link.TargetKey));

            public GraphFragment<PackageNode> PackageDependencies(string? targetFramework) => _packageDependenciesCompatibility.Resolve(targetFramework)
                .SelectMany(tfm => Outputs(PackageLabels.PackageDependency, tfm))
                .Select(link => link.Target().AsPackage()).OfType<PackageNode>()
                .ToFragment(packageVersionNode => packageVersionNode.PackageDependencies(targetFramework));

            private NuGetCompatibility GetCompatibility(GraphLabel label) => 
                new(Outputs(label).SelectMany(link => link.Labels).Select(label => label.ToString()));
        }

        public class PackageKey : GraphKey
        {
            public static PackageKey Parse(string packageKey)
            {
                PackageKey key;

                string[] parts = packageKey.Split(':');
                if (parts.Length == 2)
                    key = new(parts[0], parts[1]);
                else
                    key = new(packageKey, null);

                return key;
            }

            public PackageKey(string packageId, string? packageVersion)
                : base($"{packageId}:{packageVersion}")
            {
                Id = packageId;
                Version = packageVersion;
            }

            public string Id { get; }

            public string? Version { get; }
        }

        public static class PackageLabels
        {
            public const string Package = nameof(Package);
            public const string PackageAssembly = nameof(PackageAssembly);
            public const string PackageDependency = nameof(PackageDependency);
            
            public const string FrameworkDependency = nameof(FrameworkDependency);
            public const string FrameworkReference = nameof(FrameworkReference);
        }
    }
}
