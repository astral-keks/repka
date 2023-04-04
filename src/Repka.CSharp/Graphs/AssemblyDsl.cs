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

            public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

            public string Path => Key;
        }

        public static class AssemblyLabels
        {
            public const string Assembly = nameof(Assembly);
        }
    }
}
