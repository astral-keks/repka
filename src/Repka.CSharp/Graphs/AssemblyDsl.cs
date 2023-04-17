using Repka.Assemblies;
using Repka.Paths;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class AssemblyDsl
    {
        public static IEnumerable<AssemblyNode> Assemblies(this Graph graph) => graph.Nodes()
            .Select(node => node.AsAssembly()).OfType<AssemblyNode>();

        public static IEnumerable<AssemblyNode> Assemblies(this Graph graph, string name) => graph.Assemblies()
            .Where(assembly => string.Equals(assembly.Name, name, StringComparison.OrdinalIgnoreCase));

        public static AssemblyNode? Assembly(this Graph graph, GraphKey key) => graph.Node(key).AsAssembly();

        public static AssemblyNode? AsAssembly(this GraphNode? node) => node?.Labels.Contains(AssemblyLabels.Assembly) == true
            ? new AssemblyNode(node)
            : default;

        public class AssemblyNode : GraphNode
        {
            internal AssemblyNode(GraphNode node) : base(node) { }

            public AbsolutePath Location => new(Key);

            public string? Name => Metadata.Name;

            public Version? Version => Metadata.Version;

            public AssemblyMetadata Metadata => Attribute(AssemblyAttributes.Metadata)
                .Value(() => new AssemblyMetadata(Location));

            public PackageNode? Package() => Inputs(AssemblyLabels.Assembly)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>()
                .SingleOrDefault();

            public IEnumerable<ProjectNode> ReferencingProjects() => Inputs(AssemblyLabels.AssemblyReference)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();

            public IEnumerable<PackageNode> ReferencingPackages() => Inputs(AssemblyLabels.AssemblyReference)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>();
        }

        public static class AssemblyLabels
        {
            public const string Assembly = nameof(Assembly);
            public const string AssemblyReference = $"{Assembly}.{nameof(AssemblyReference)}";
        }

        public static class AssemblyAttributes
        {
            public const string Metadata = nameof(Metadata);
        }
    }
}
