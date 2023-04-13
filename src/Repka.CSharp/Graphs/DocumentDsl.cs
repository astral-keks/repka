using Repka.Paths;
using static Repka.Graphs.ProjectDsl;
using static Repka.Graphs.SymbolDsl;

namespace Repka.Graphs
{
    public static class DocumentDsl
    {
        public static IEnumerable<DocumentNode> Documents(this Graph graph) => graph.Nodes()
            .Select(node => node.AsDocument())
            .OfType<DocumentNode>();

        public static DocumentNode? Document(this Graph graph, string? path) => path is not null ? graph.Document(new GraphKey(path)) : default;

        public static DocumentNode? Document(this Graph graph, GraphKey key) => graph.Node(key).AsDocument();

        public static DocumentNode? AsDocument(this GraphNode? node) => node?.Labels.Contains(DocumentLabels.Document) == true
            ? new DocumentNode(node)
            : default;

        public class DocumentNode : GraphNode
        {
            internal DocumentNode(GraphNode node) : base(node) { }

            public string Name => Path.GetFileName(Location);

            public AbsolutePath Location => new(Key);

            public FileInfo File() => new(Location);

            public ProjectNode Project => Projects.Single();

            public IEnumerable<ProjectNode> Projects => Inputs(DocumentLabels.Document)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();

            public IEnumerable<SymbolNode> DefinedSymbols => Outputs(SymbolLabels.DefinesSymbol)
                .Select(link => link.Target().AsSymbol()).OfType<SymbolNode>();

            public IEnumerable<SymbolNode> ReferencedSymbols => Outputs(SymbolLabels.ReferencesSymbol)
                .Select(link => link.Target().AsSymbol()).OfType<SymbolNode>();

            public Stream Read() => System.IO.File.OpenRead(Location);
        }

        public static class DocumentLabels
        {
            public const string Document = nameof(Document);
        }
    }
}
