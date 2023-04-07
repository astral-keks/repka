using Repka.Assemblies;
using System.Linq;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class AssemblyDsl
    {
        public static IEnumerable<AssemblyNode> Assemblies(this ProjectNode project) => project.Outputs(AssemblyLabels.Assembly)
            .Select(link => link.Target().AsAssembly()).OfType<AssemblyNode>();

        public static IEnumerable<AssemblyNode> Assemblies(this Graph graph) => graph.Nodes()
            .Select(node => node.AsAssembly()).OfType<AssemblyNode>();

        public static AssemblyNode? Assembly(this Graph graph, GraphKey key) => graph.Node(key).AsAssembly();

        public static AssemblyNode? AsAssembly(this GraphNode? node) => node?.Labels.Contains(AssemblyLabels.Assembly) == true
            ? new AssemblyNode(node)
            : default;

        public class AssemblyNode : GraphNode
        {
            internal AssemblyNode(GraphNode node) : base(node) 
            {
                _descriptpor = new(GetDescriptor);
            }

            public string Location => Key;

            private readonly Lazy<AssemblyDescriptor> _descriptpor;
            public AssemblyDescriptor Descriptor => _descriptpor.Value;
            private AssemblyDescriptor GetDescriptor() => new(Location);

            public string Name => Path.GetFileNameWithoutExtension(Location);

            public PackageNode? Package => Inputs(PackageLabels.PackageAssembly)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>()
                .SingleOrDefault();

            public IEnumerable<ProjectNode> Projects => Inputs(AssemblyLabels.Assembly)
                .Select(link => link.Source()).OfType<ProjectNode>();
        }

        public static class AssemblyLabels
        {
            public const string Assembly = nameof(Assembly);
        }
    }
}
