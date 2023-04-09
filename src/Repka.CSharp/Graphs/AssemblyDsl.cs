using Repka.Assemblies;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class AssemblyDsl
    {
        public static IEnumerable<AssemblyNode> Assemblies(this Graph graph) => graph.Nodes()
            .Select(node => node.AsAssembly()).OfType<AssemblyNode>();

        public static AssemblyNode? Assembly(this Graph graph, GraphKey key) => graph.Node(key).AsAssembly();

        public static AssemblyNode? AsAssembly(this GraphNode? node) => node?.Labels.Contains(AssemblyLabels.Assembly) == true
            ? new AssemblyNode(node)
            : default;


        public static IEnumerable<AssemblyNode> AssemblyDependencies(this ProjectNode project) => project.Outputs(AssemblyLabels.AssemblyDependency)
            .Select(link => link.Target().AsAssembly()).OfType<AssemblyNode>();

        public static IEnumerable<AssemblyNode> AssemblyDependencies(this PackageNode package) => package.Outputs(AssemblyLabels.AssemblyDependency)
            .Select(link => link.Target().AsAssembly()).OfType<AssemblyNode>();


        public class AssemblyNode : GraphNode
        {
            internal AssemblyNode(GraphNode node) : base(node) 
            {
            }

            public string Location => Key;

            public string? Name => Descriptor.Name;

            public Version? Version => Descriptor.Version;

            public AssemblyDescriptor Descriptor => Attribute(AssemblyAttributes.Descriptor)
                .Value(() => new AssemblyDescriptor(Location));

            public PackageNode? Package => Inputs(AssemblyLabels.Assembly)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>()
                .SingleOrDefault();

            public IEnumerable<ProjectNode> DependentProjects => Inputs(AssemblyLabels.AssemblyDependency)
                .Select(link => link.Source()).OfType<ProjectNode>();

            public IEnumerable<PackageNode> DependentPackages => Inputs(AssemblyLabels.AssemblyDependency)
                .Select(link => link.Source()).OfType<PackageNode>();
        }

        public static class AssemblyLabels
        {
            public const string Assembly = nameof(Assembly);
            public const string AssemblyDependency = nameof(AssemblyDependency);
        }

        public static class AssemblyAttributes
        {
            public const string Descriptor = nameof(Descriptor);
        }
    }
}
