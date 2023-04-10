namespace Repka.Graphs
{
    public static class FileSystemDsl
    {
        public static IEnumerable<FileNode> Files(this Graph graph) => graph.Nodes()
            .Select(node => node.AsFile())
            .OfType<FileNode>();

        public static FileNode? File(this Graph graph, string? path) => path is not null ? graph.File(new GraphKey(path)) : default;

        public static FileNode? File(this Graph graph, GraphKey key) => graph.Node(key).AsFile();

        public static FileNode? AsFile(this GraphNode? node) => node?.Labels.Contains(FileSystemLabels.File) == true
            ? new FileNode(node)
            : default;


        public static IEnumerable<DirectoryNode> Directories(this Graph graph) => graph.Nodes()
            .Select(node => node.AsDirectory())
            .OfType<DirectoryNode>();

        public static DirectoryNode? Directory(this Graph graph, string? path) => path is not null ? graph.Directory(new GraphKey(path)) : default;

        public static DirectoryNode? Directory(this Graph graph, GraphKey key) => graph.Node(key).AsDirectory();

        public static DirectoryNode? AsDirectory(this GraphNode? node) => node?.Labels.Contains(FileSystemLabels.Directory) == true
            ? new DirectoryNode(node)
            : default;


        public class FileNode : FileSystemNode
        {
            internal FileNode(GraphNode node) : base(node) { }
        }

        public class DirectoryNode : FileSystemNode
        {
            internal DirectoryNode(GraphNode node) : base(node) { }

        }

        public class FileSystemNode : GraphNode
        {
            internal FileSystemNode(GraphNode node) : base(node) { }

            public string Path => Key;

            public IEnumerable<GraphNode> Referers => Inputs(FileSystemLabels.Reference)
                .Select(link => link.Target()).OfType<GraphNode>();

            public IEnumerable<GraphNode> References => Outputs(FileSystemLabels.Reference)
                .Select(link => link.Target().AsDirectory()).OfType<GraphNode>();
        }

        public static class FileSystemLabels
        {
            public const string Directory = nameof(Directory);
            public const string File = nameof(File);
            public const string Reference = nameof(Reference);
        }
    }
}
