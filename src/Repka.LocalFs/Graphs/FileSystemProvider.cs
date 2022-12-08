using Repka.FileSystems;

namespace Repka.Graphs
{
    public class FileSystemProvider : GraphProvider
    {
        public FileSystem FileSystem { get; init; } = FileSystemDefinitions.Empty();

        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            FileSystemGrouping grouping = new();

            FileSystemEntry root = new(key);
            foreach (var entry in FileSystem.GetEntries(root))
            {
                GraphKey? sourceKey = entry.Origin is not null ? new(entry.Origin) : default;
                GraphKey targetKey = new(entry.Path);

                GraphToken? target = GetNodeToken(targetKey, grouping);
                if (target is not null)
                    yield return target;

                if (sourceKey is not null)
                {
                    GraphToken? source = GetNodeToken(sourceKey, grouping);
                    if (source is not null)
                        yield return source;

                    yield return new GraphLinkToken(sourceKey, targetKey, FileSystemLabels.FsRef);
                }
            }

            foreach (var (directory, items) in grouping)
            {
                if (items.Count > 1)
                {
                    yield return new GraphNodeToken(directory, FileSystemLabels.Directory);
                    foreach (var item in items)
                    {
                        yield return new GraphLinkToken(directory, item, FileSystemLabels.FsRef);
                    }
                }
            }
        }

        public override IEnumerable<GraphAttribute> GetAttributes(GraphToken token, Graph graph)
        {
            if (graph.Element(token) is GraphNode dirNode && dirNode.Labels.Contains(FileSystemLabels.Directory))
            {
                string path = dirNode.Key.Resource;

                yield return new GraphAttribute<DirectoryInfo>(token, new DirectoryInfo(path));
            }
            else if (graph.Element(token) is GraphNode fileNode && fileNode.Labels.Contains(FileSystemLabels.File))
            {
                string path = fileNode.Key.Resource;

                yield return new GraphAttribute<FileInfo>(token, new FileInfo(path));
            }
        }

        private GraphToken? GetNodeToken(GraphKey key, FileSystemGrouping grouping)
        {
            GraphNodeToken? token = default;

            if (File.Exists(key))
            {
                token = new GraphNodeToken(key, FileSystemLabels.File);
            }
            else if (Directory.Exists(key))
            {
                token = new GraphNodeToken(key, FileSystemLabels.Directory);
            }

            if (token != null)
                grouping.Add(token.Key);

            return token;
        }

    }
}
