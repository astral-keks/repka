using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class DocumentDsl
    {
        public static DocumentNode? Document(this Graph graph, GraphKey key) => graph.Node(key).AsDocument();

        public static IEnumerable<DocumentNode> Documents(this Graph graph) => graph.Nodes()
            .Select(node => node.AsDocument())
            .OfType<DocumentNode>();

        public static DocumentNode? AsDocument(this GraphNode? node) => node?.Labels.Contains(DocumentLabels.Document) == true
            ? new DocumentNode(node)
            : default;

        public class DocumentNode : GraphNode
        {
            internal DocumentNode(GraphNode node) : base(node) { }

            public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

            public string Path => Key;

            public ProjectNode Project => Projects.Single();

            public IEnumerable<ProjectNode> Projects => Inputs(DocumentLabels.Document)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();

            public Stream Read() => File.OpenRead(Path);
        }

        public static class DocumentLabels
        {
            public const string Document = nameof(Document);
        }
    }
}
