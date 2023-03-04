namespace Repka.Graphs
{
    public static class SymbolDsl
    {
        public static IEnumerable<SymbolNode> Symbols(this Graph graph) => graph.Nodes()
            .Select(SymbolNode.Of)
            .OfType<SymbolNode>();

        public static SymbolNode? Symbol(this GraphNode node) => SymbolNode.Of(node);

        public class SymbolNode : GraphNode
        {
            public static SymbolNode? Of(GraphNode? node) =>
                node?.Labels.Contains(CSharpLabels.IsSymbol) == true ? new(node) : default;

            public SymbolNode(GraphNode node) : base(node) {}

            public GraphKey Name => Key;

            public bool IsType => Labels.Contains(CSharpLabels.IsType);

            public bool IsField => Labels.Contains(CSharpLabels.IsField);

            public bool IsProperty => Labels.Contains(CSharpLabels.IsProperty);

            public bool IsMethod => Labels.Contains(CSharpLabels.IsMethod);

            public IEnumerable<SymbolFile> Origins => Inputs(CSharpLabels.DefinesSymbol)
                .Select(input => new SymbolFile(input.SourceKey, input.Graph));

            public IEnumerable<SymbolFile> Referers => Inputs(CSharpLabels.UsesSymbol)
                .Select(input => new SymbolFile(input.SourceKey, input.Graph));
        }

        public class SymbolFile
        {
            private readonly GraphKey _key;
            private readonly Graph _graph;

            public SymbolFile(GraphKey key, Graph graph)
            {
                _key = key;
                _graph = graph;
            }

            public GraphKey Name => _key;

            public IEnumerable<SymbolNode> Symbols() => _graph.Links(_key)
                .Select(link => link.Target())
                .Select(SymbolNode.Of)
                .OfType<SymbolNode>();
        }
    }
}
