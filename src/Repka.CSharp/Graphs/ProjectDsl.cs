using Microsoft.CodeAnalysis;
using Repka.Assemblies;
using Repka.Packaging;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.SolutionDsl;

namespace Repka.Graphs
{
    public static class ProjectDsl
    {
        public static IEnumerable<ProjectNode> Projects(this Graph graph) => graph.Nodes()
            .Select(node => node.AsProject()).OfType<ProjectNode>();

        public static ProjectNode? Project(this Graph graph, GraphKey key) => graph.Node(key).AsProject();

        public static ProjectNode? AsProject(this GraphNode? node) =>
            node?.Labels.Contains(ProjectLabels.Project) == true ? new(node) : default;


        public class ProjectNode : GraphNode
        {
            internal ProjectNode(GraphNode node) : base(node) { }

            public ProjectId Id => ProjectId.CreateFromSerialized(Key.GetGuid());

            public string Name => Path.GetFileNameWithoutExtension(Key);

            public string Directory => Path.GetDirectoryName(Key) 
                ?? throw new DirectoryNotFoundException("Project directory could not be resolved");

            public string Location => Key;

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


            public IEnumerable<string> DocumentReferences => Outputs(ProjectLabels.DocumentReference)
                .Select(link => link.TargetKey.ToString());

            public IEnumerable<DocumentNode> Documents => Outputs(DocumentLabels.Document)
                .Select(link => link.Target().AsDocument())
                .OfType<DocumentNode>();


            public IEnumerable<string> FrameworkReferences => Outputs(ProjectLabels.FrameworkReference)
                .Select(link => link.TargetKey.ToString());


            public IEnumerable<AssemblyDescriptor> LibraryReferences => Outputs(ProjectLabels.LibraryReference)
                .Select(link => new AssemblyDescriptor(link.TargetKey.ToString()));


            public IEnumerable<string> ProjectReferences => Outputs(ProjectLabels.ProjectReference)
                .Select(link => link.TargetKey.ToString());

            public IEnumerable<ProjectNode> ProjectDependencies => Outputs(ProjectLabels.ProjectDependency)
                .Select(link => link.Target().AsProject()).OfType<ProjectNode>();

            public IEnumerable<ProjectNode> DependentProjects => Inputs(ProjectLabels.ProjectDependency)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();
            

            public IEnumerable<NuGetDescriptor> PackageReferences => Outputs(ProjectLabels.PackageReference)
                .Select(link => NuGetDescriptor.Parse(link.TargetKey));

            public IEnumerable<PackageNode> PackageDependencies => Outputs(PackageLabels.PackageDependency)
                .Select(link => link.Target().AsPackage()).OfType<PackageNode>();


            public IEnumerable<SolutionNode> Solutions => Inputs(SolutionLabels.SolutionProject)
                .Select(link => link.Source().AsSolution()).OfType<SolutionNode>();
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

            public const string LibraryReference = nameof(LibraryReference);

            public const string ProjectReference = nameof(ProjectReference);
            public const string ProjectDependency = nameof(ProjectDependency);
            
            public const string PackageReference = nameof(PackageReference);

            public const string DocumentReference = nameof(DocumentReference);
        }
    }
}
