using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class AssemblyDsl
    {
        public static AssemblyNode? Assembly(this Graph graph, GraphKey key) => graph.Node(key).AsAssembly();

        public static IEnumerable<AssemblyNode> Assemblies(this Graph graph) => graph.Nodes()
            .Select(node => node.AsAssembly()).OfType<AssemblyNode>();

        public static AssemblyNode? AsAssembly(this GraphNode? node) => node?.Labels.Contains(AssemblyLabels.Assembly) == true
            ? new AssemblyNode(node)
            : default;

        public class AssemblyNode : GraphNode
        {
            internal AssemblyNode(GraphNode node) : base(node) { }

            public string Name => Path.GetFileNameWithoutExtension(Location);

            public string Location => Key;

            public PackageNode? Package => Inputs(PackageLabels.PackageAssembly)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>()
                .SingleOrDefault();

            public IEnumerable<PackageNode> ReferencingPackages => Inputs(PackageLabels.FrameworkDependency)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>();

            public IEnumerable<GraphNode> ReferencingProjects => Inputs()
                .Where(link => link.Labels.ContainsAny(ProjectLabels.LibraryReference, ProjectLabels.FrameworkDependency))
                .Select(link => link.Source()).OfType<GraphNode>();

            public IEnumerable<GraphNode> DependingProjects => Inputs(AssemblyLabels.AssemblyDependency)
                .Select(link => link.Source()).OfType<GraphNode>();
        }

        public static class AssemblyLabels
        {
            public const string Assembly = nameof(Assembly);
            public const string AssemblyDependency = nameof(AssemblyDependency);
        }
    }
}
