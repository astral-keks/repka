using Repka.FileSystems;

namespace Repka.Graphs
{
    public class FileSystemProvider : GraphProvider
    {
        public FileSystem FileSystem { private get; init; } = FileSystemDefinitions.Empty();

        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            FileSystemGrouping grouping = new();
            FileSystemEntry root = new(key);

            int i = 0;
            Progress.Start("Files and folders");
            foreach (var entry in FileSystem.GetEntries(root))
            {
                Progress.Notify($"Files and folders: {++i}");
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
            Progress.Finish($"Files and folders: {i}");
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
