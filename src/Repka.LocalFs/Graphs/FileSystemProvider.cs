using Repka.Collections;
using Repka.Diagnostics;
using Repka.FileSystems;
using System.Collections;
using static Repka.Graphs.FileSystemDsl;

namespace Repka.Graphs
{
    public class FileSystemProvider : GraphProvider
    {
        public FileSystem FileSystem { private get; init; } = FileSystemDefinitions.Empty();

        public override void AddTokens(GraphKey key, Graph graph)
        {
            FileSystemEntry root = new(key);

            ProgressCounter progress = Progress.Count("Files system entries");
            IEnumerable<FileSystemEntry> entries = FileSystem.GetEntries(root);
            foreach (var token in GetFileSystemTokens(entries.Peek(progress.Increment)))
                graph.Add(token);
            progress.Complete();
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

                    yield return new GraphLinkToken(sourceKey, targetKey, FileSystemLabels.Reference);
                }
            }

            foreach (var (directory, items) in grouping)
            {
                if (items.Count > 1)
                {
                    yield return new GraphNodeToken(directory, FileSystemLabels.Directory);
                    foreach (var item in items)
                    {
                        yield return new GraphLinkToken(directory, item, FileSystemLabels.Reference);
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

    internal class FileSystemGrouping : IEnumerable<(string Directory, HashSet<GraphKey> Items)>
    {
        private readonly Dictionary<string, HashSet<GraphKey>> _keysByDirectory = new();

        public void Add(GraphKey key)
        {
            string? directory = Path.GetDirectoryName(key);
            if (directory is not null)
            {
                if (!_keysByDirectory.ContainsKey(directory))
                    _keysByDirectory[directory] = new();
                _keysByDirectory[directory].Add(key);
            }
        }

        public IEnumerator<(string Directory, HashSet<GraphKey> Items)> GetEnumerator()
        {
            return _keysByDirectory.Select(entry => (entry.Key, entry.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
