using Microsoft.CodeAnalysis;
using Repka.Assemblies;
using Repka.Packaging;
using Repka.Paths;
using static Repka.Graphs.AssemblyDsl;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.SolutionDsl;

namespace Repka.Graphs
{
    public static class ProjectDsl
    {
        public static IEnumerable<ProjectNode> Projects(this Graph graph) => graph.Nodes()
            .Select(node => node.AsProject()).OfType<ProjectNode>();

        public static IEnumerable<ProjectNode> Projects(this Graph graph, string name) => graph.Projects()
            .Where(project => string.Equals(project.Name, name, StringComparison.OrdinalIgnoreCase));

        public static ProjectNode? Project(this Graph graph, GraphKey key) => graph.Node(key).AsProject();

        public static ProjectNode? AsProject(this GraphNode? node) =>
            node?.Labels.Contains(ProjectLabels.Project) == true ? new(node) : default;

        public class ProjectNode : GraphNode
        {
            internal ProjectNode(GraphNode node) : base(node) { }

            public string Name => Path.GetFileNameWithoutExtension(Key);

            public ProjectId Id => ProjectId.CreateFromSerialized(Key.AsGuid(), Location);

            public AbsolutePath Directory => Path.GetDirectoryName(Key) 
                ?? throw new DirectoryNotFoundException("Project directory could not be resolved");

            public AbsolutePath Location => new(Key);

            public bool HasSdk => !string.IsNullOrWhiteSpace(Sdk);

            public string? Sdk => Outputs(ProjectLabels.Sdk)
                .Select(link => link.TargetKey.ToString())
                .FirstOrDefault();

            public IEnumerable<string> TargetFrameworks => Tags(ProjectLabels.TargetFramework)
                .Select(tag => tag.Value);

            public string AssemblyName => Tag(ProjectLabels.AssemblyName).OrElse(Name);

            public bool IsPackageable => PackageId is not null;

            public NuGetIdentifier? PackageId => Outputs(ProjectLabels.Package)
                .Select(link => NuGetDescriptor.Parse(link.TargetKey))
                .Select(descriptor => descriptor.Id)
                .FirstOrDefault();

            public PackageNode? Package() => Outputs(ProjectLabels.Package)
                .Select(link => link.Target().AsPackage()).OfType<PackageNode>()
                .FirstOrDefault();


            public IEnumerable<AbsolutePath> DocumentReferences => Outputs(ProjectLabels.DocumentReference)
                .Select(link => link.TargetKey.AsAbsolutePath());

            public IEnumerable<DocumentNode> ReferencedDocuments() => Outputs(ProjectLabels.DocumentReference)
                .Select(link => link.Target().AsDocument()).OfType<DocumentNode>();

            public IEnumerable<DocumentNode> Documents() => Outputs(DocumentLabels.Document)
                .Select(link => link.Target().AsDocument()).OfType<DocumentNode>();


            public IEnumerable<AssemblyName> AssemblyReferences => Outputs(ProjectLabels.AssemblyReference)
                .Select(link => link.TargetKey.AsAssemblyName());


            public IEnumerable<AbsolutePath> LibraryReferences => Outputs(ProjectLabels.LibraryReference)
                .Select(link => link.TargetKey.AsAbsolutePath());

            public IEnumerable<AssemblyNode> ReferencedLibraries() => Outputs(ProjectLabels.LibraryReference)
                .Select(link => link.Target().AsAssembly()).OfType<AssemblyNode>();


            public IEnumerable<AbsolutePath> ProjectReferences => Outputs(ProjectLabels.ProjectReference)
                .Select(link => link.TargetKey.AsAbsolutePath());
            
            public IEnumerable<ProjectNode> ReferencedProjects() => Outputs(ProjectLabels.ProjectReference)
                .Select(link => link.Target().AsProject()).OfType<ProjectNode>();

            public IEnumerable<ProjectNode> ReferencingProjects() => Inputs(ProjectLabels.ProjectReference)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();


            public IEnumerable<NuGetDescriptor> PackageReferences => Outputs(ProjectLabels.PackageReference)
                .Select(link => NuGetDescriptor.Parse(link.TargetKey));
            
            public IEnumerable<PackageNode> ReferencedPackages() => Outputs(PackageLabels.ReferencedPackage)
                .Select(link => link.Target().AsPackage()).OfType<PackageNode>();


            public IEnumerable<AssemblyNode> RestoredAssemblies() => Outputs(AssemblyLabels.Restored)
                .Select(link => link.Target().AsAssembly()).OfType<AssemblyNode>();


            public IEnumerable<SolutionNode> Solutions() => Inputs(SolutionLabels.SolutionProject)
                .Select(link => link.Source().AsSolution()).OfType<SolutionNode>();
        }

        public static class ProjectLabels
        {
            public const string Project = nameof(Project);
            public const string Executable = $"{Project}.{nameof(Executable)}";
            public const string Library = $"{Project}.{nameof(Library)}";

            public const string Sdk = $"{Project}.{nameof(Sdk)}";
            public const string AssemblyName = $"{Project}.{nameof(AssemblyName)}";
            public const string TargetFramework = $"{Project}.{nameof(TargetFramework)}";

            public const string Package = $"{Project}.{nameof(Package)}";

            public const string AssemblyReference = $"{Project}.{nameof(AssemblyReference)}";

            public const string LibraryReference = $"{Project}.{nameof(LibraryReference)}";

            public const string ProjectReference = $"{Project}.{nameof(ProjectReference)}";
            public const string ProjectDependency = $"{Project}.{nameof(ProjectDependency)}";

            public const string TransitiveDependency = $"{Project}.{nameof(TransitiveDependency)}";
            public const string DirectDependency = $"{Project}.{nameof(DirectDependency)}";
            
            public const string PackageReference = $"{Project}.{nameof(PackageReference)}";

            public const string DocumentReference = $"{Project}.{nameof(DocumentReference)}";
        }
    }
}
