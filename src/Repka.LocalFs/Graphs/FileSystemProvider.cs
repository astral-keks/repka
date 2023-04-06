using Repka.Collections;
using Repka.FileSystems;

namespace Repka.Graphs
{
    public class FileSystemProvider : GraphProvider
    {
        public FileSystem FileSystem { private get; init; } = FileSystemDefinitions.Empty();

        public override void AddTokens(GraphKey key, Graph graph)
        {
            FileSystemEntry root = new(key);

            int i = 0;
            Progress.Start("Files and folders");
            foreach (var token in GetFileSystemTokens(FileSystem.GetEntries(root).Peek(() => Progress.Notify($"Files and folders: {++i}"))))
                graph.Add(token);
            Progress.Finish($"Files and folders: {i}");
        }

        private IEnumerable<GraphToken> GetFileSystemTokens(IEnumerable<FileSystemEntry> entries)
        {
            FileSystemGrouping grouping = new();

            foreach (var entry in entries)
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
