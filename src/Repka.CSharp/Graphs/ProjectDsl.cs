using Microsoft.CodeAnalysis;
using Repka.Assemblies;
using Repka.Packaging;
using static Repka.Graphs.AssemblyDsl;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.PackageDsl;

namespace Repka.Graphs
{
    public static class ProjectDsl
    {
        public static ProjectNode? Project(this Graph graph, GraphKey key) => graph.Node(key).AsProject();

        public static IEnumerable<ProjectNode> Projects(this Graph graph) => graph.Nodes()
            .Select(node => node.AsProject()).OfType<ProjectNode>();

        public static ProjectNode? AsProject(this GraphNode? node) =>
            node?.Labels.Contains(ProjectLabels.Project) == true ? new(node) : default;

        public static IEnumerable<ProjectNode> ProjectReferences(this ProjectNode project) => project.Outputs(ProjectLabels.ProjectReference)
            .Select(link => link.Target().AsProject()).OfType<ProjectNode>();

        public static IEnumerable<PackageNode> PackageReferences(this ProjectNode project) => project.Outputs(ProjectLabels.PackageReference)
            .Select(link => link.Target().AsPackage())
            .OfType<PackageNode>();

        public class ProjectNode : GraphNode
        {
            internal ProjectNode(GraphNode node) : base(node) { }

            public ProjectId Id => ProjectId.CreateFromSerialized(Key.GetGuid());

            public string Name => System.IO.Path.GetFileNameWithoutExtension(Key);

            public string Directory => System.IO.Path.GetDirectoryName(Key) 
                ?? throw new DirectoryNotFoundException("Project directory could not be resolved");

            public string Path => Key;

            public bool HasSdk => !string.IsNullOrWhiteSpace(Sdk);

            public string? Sdk => Outputs(ProjectLabels.Sdk)
                .Select(link => link.TargetKey.ToString())
                .FirstOrDefault();

            public string? TargetFramework => Outputs(ProjectLabels.TargetFramework)
                .Select(link => link.TargetKey.ToString())
                .FirstOrDefault();

            public NuGetIdentifier? PackageId => Outputs(ProjectLabels.PackageDefinition)
                .Select(link => new NuGetIdentifier(link.TargetKey.ToString()))
                .FirstOrDefault();

            public PackageNode? Package => Outputs(ProjectLabels.PackageDefinition)
                .Select(link => link.Target().AsPackage()).OfType<PackageNode>()
                .FirstOrDefault();

            public IEnumerable<string> DocumentLinks => Outputs(ProjectLabels.DocumentLink)
                .Select(link => link.TargetKey.ToString());

            public IEnumerable<DocumentNode> Documents => Outputs(DocumentLabels.Document)
                .Select(link => link.Target().AsDocument())
                .OfType<DocumentNode>();

            public IEnumerable<string> FrameworkReferences => Outputs(ProjectLabels.FrameworkReference)
                .Select(link => link.TargetKey.ToString());

            public IEnumerable<AssemblyFile> FrameworkDependencies => Outputs(ProjectLabels.FrameworkDependency)
                .Select(link => new AssemblyFile(link.TargetKey));

            public IEnumerable<string> LibraryReferences => Outputs(ProjectLabels.LibraryReference)
                .Select(link => link.TargetKey.ToString());

            public IEnumerable<AssemblyFile> LibraryDependencies => Outputs(ProjectLabels.LibraryReference)
                .Select(link => new AssemblyFile(link.TargetKey.ToString()));

            public IEnumerable<string> ProjectReferences => Outputs(ProjectLabels.ProjectReference)
                .Select(link => link.TargetKey.ToString());

            public IEnumerable<NuGetDescriptor> PackageReferences => Outputs(ProjectLabels.PackageReference)
                .Select(link => PackageKey.Parse(link.TargetKey))
                .Select(key => NuGetDescriptor.Of(key.Id, key.Version));

            public GraphFragment<ProjectNode> ProjectDependencies => Outputs(ProjectLabels.ProjectDependency)
                .Select(link => link.Target().AsProject()).OfType<ProjectNode>()
                .ToFragment(projectNode => projectNode.ProjectDependencies);

            public GraphFragment<PackageNode> PackageDependencies(string? targetFramework) => Outputs(PackageLabels.PackageDependency)
                .Select(link => link.Target().AsPackage()).OfType<PackageNode>()
                .ToFragment(packageNode => packageNode.PackageDependencies(targetFramework));

            public IEnumerable<AssemblyFile> AssemblyDependencies => Outputs(AssemblyLabels.AssemblyDependency)
                .Select(link => new AssemblyFile(link.TargetKey.ToString()));
        }

        public static class ProjectLabels
        {
            public const string Project = nameof(Project);
            public const string PackageProject = nameof(PackageProject);
            public const string ExecutableProject = nameof(ExecutableProject);
            public const string LibraryProject = nameof(LibraryProject);

            public const string Sdk = nameof(Sdk);
            public const string TargetFramework = nameof(TargetFramework);

            public const string PackageDefinition = nameof(PackageDefinition);

            public const string FrameworkReference = nameof(FrameworkReference);
            public const string FrameworkDependency = nameof(FrameworkDependency);

            public const string LibraryReference = nameof(LibraryReference);

            public const string ProjectReference = nameof(ProjectReference);
            public const string ProjectDependency = nameof(ProjectDependency);

            public const string PackageReference = nameof(PackageReference);

            public const string DocumentLink = nameof(DocumentLink);

        }
    }
}
