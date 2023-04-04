using static Repka.Graphs.DocumentDsl;

namespace Repka.Graphs
{
    public static class SymbolDsl
    {
        public static SymbolNode? Symbol(this Graph graph, GraphKey key) => graph.Node(key).AsSymbol();

        public static IEnumerable<SymbolNode> Symbols(this Graph graph) => graph.Nodes()
            .Select(node => node.AsSymbol())
            .OfType<SymbolNode>();

        public static SymbolNode? AsSymbol(this GraphNode? node) => 
            node?.Labels.Contains(SymbolLabels.IsSymbol) == true ? new(node) : default;

        public class SymbolNode : GraphNode
        {
            public SymbolNode(GraphNode node) : base(node) { }

            public GraphKey Name => Key;

            public bool IsType => Labels.Contains(SymbolLabels.IsType);

            public bool IsField => Labels.Contains(SymbolLabels.IsField);

            public bool IsProperty => Labels.Contains(SymbolLabels.IsProperty);

            public bool IsMethod => Labels.Contains(SymbolLabels.IsMethod);

            public DocumentNode Definition => Definitions.Single();
 
            public IEnumerable<DocumentNode> Definitions => Inputs(SymbolLabels.DefinesSymbol)
                .Select(link => link.Source().AsDocument()).OfType<DocumentNode>();

            public IEnumerable<DocumentNode> References => Inputs(SymbolLabels.UsesSymbol)
                .Select(link => link.Source().AsDocument()).OfType<DocumentNode>();
        }

        public static class SymbolLabels
        {
            public const string IsSymbol = nameof(IsSymbol);
            public const string IsType = nameof(IsType);
            public const string IsField = nameof(IsField);
            public const string IsProperty = nameof(IsProperty);
            public const string IsMethod = nameof(IsMethod);

            public const string DefinesSymbol = nameof(DefinesSymbol);

            public const string UsesSymbol = nameof(UsesSymbol);
        }
    }
}
