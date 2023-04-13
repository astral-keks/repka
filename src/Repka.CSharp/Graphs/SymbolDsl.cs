using static Repka.Graphs.DocumentDsl;

namespace Repka.Graphs
{
    public static class SymbolDsl
    {
        public static IEnumerable<SymbolNode> Symbols(this Graph graph) => graph.Nodes()
            .Select(node => node.AsSymbol())
            .OfType<SymbolNode>();

        public static SymbolNode? Symbol(this Graph graph, GraphKey key) => graph.Node(key).AsSymbol();

        public static SymbolNode? AsSymbol(this GraphNode? node) =>
            node?.Labels.Contains(SymbolLabels.Symbol) == true ? new(node) : default;

        public class SymbolNode : GraphNode
        {
            public SymbolNode(GraphNode node) : base(node) { }

            public string Name => Key;

            public int Size => Tag(SymbolLabels.Size)
                .Map(label => int.TryParse(label.Value, out int size) ? size : 0)
                .OrElseDefault();

            public bool IsType => Labeled(SymbolLabels.IsType);

            public bool IsField => Labeled(SymbolLabels.IsField);

            public bool IsProperty => Labeled(SymbolLabels.IsProperty);

            public bool IsMethod => Labeled(SymbolLabels.IsMethod);

            public IEnumerable<DocumentNode> DefiningDocuments => Inputs(SymbolLabels.DefinesSymbol)
                .Select(link => link.Source().AsDocument()).OfType<DocumentNode>();

            public IEnumerable<DocumentNode> ReferencingDocuments => Inputs(SymbolLabels.ReferencesSymbol)
                .Select(link => link.Source().AsDocument()).OfType<DocumentNode>();
        }

        public static class SymbolLabels
        {
            public const string Symbol = nameof(Symbol);
            public const string IsType = $"{Symbol}.{nameof(IsType)}";
            public const string IsField = $"{Symbol}.{nameof(IsField)}";
            public const string IsProperty = $"{Symbol}.{nameof(IsProperty)}";
            public const string IsMethod = $"{Symbol}.{nameof(IsMethod)}";

            public const string Size = $"{Symbol}.{nameof(Size)}";

            public const string DefinesSymbol= $"{Symbol}.{nameof(DefinesSymbol)}";
            public const string ReferencesSymbol = $"{Symbol}.{nameof(ReferencesSymbol)}";
        }
    }
}
