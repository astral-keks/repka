using Microsoft.CodeAnalysis;
using Repka.Caching;
using Repka.Graphs;

namespace Repka.Symbols
{
    internal class SymbolCache : IDisposable
    {
        private readonly Cache? _cache;

        public SymbolCache(Cache? cache)
        {
            _cache = cache;
        }

        public void Dispose()
        {
            _cache?.Dispose();
        }

        public ICollection<GraphToken> GetOrAdd(Document source, Func<ICollection<GraphToken>> factory)
        {
            Lazy<ICollection<GraphToken>> tokens = new(factory);

            CacheEntry entry = new(source.FilePath ?? "???",
                () => WriteTokens(tokens.Value),
                new()
                {
                    //new CacheProperty("date", source.LastWriteTimeUtc.ToString("s")),
                    //new CacheProperty("checsum", () => source.CheckSum)
                });

            CacheEntry? cached = _cache?.GetOrAdd(entry);

            return cached is not null
                ? ReadTokens(cached.Content).ToList()
                : tokens.Value;
        }

        private CacheContent WriteTokens(IEnumerable<GraphToken> tokens)
        {
            List<string> lines = new();

            foreach (var token in tokens)
            {
                if (token is GraphNodeToken node)
                    lines.Add(WriteParts(node.Key, WriteLabels(node.Labels)));
                else if (token is GraphLinkToken link)
                    lines.Add(WriteParts(link.SourceKey, link.TargetKey, WriteLabels(link.Labels)));
            }

            return new CacheContent(lines);
        }

        private IEnumerable<GraphToken> ReadTokens(CacheContent content)
        {
            foreach (var line in content.Lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = ReadParts(line);
                    if (!parts.Any(string.IsNullOrWhiteSpace))
                    {
                        if (parts.Length == 2)
                        {
                            GraphKey key = new(parts[0]);
                            GraphLabel[] labels = ReadLabels(parts[1]).ToArray();
                            yield return new GraphNodeToken(key, labels);
                        }
                        else if (parts.Length == 3)
                        {
                            GraphKey sourceKey = new(parts[0]);
                            GraphKey targetKey = new(parts[1]);
                            GraphLabel[] labels = ReadLabels(parts[2]).ToArray();
                            yield return new GraphLinkToken(sourceKey, targetKey, labels);
                        }
                    }
                }
            }
        }

        private string[] ReadParts(string text)
        {
            return text.Split('\t');
        }


        private string WriteParts(params string[] parts)
        {
            return string.Join("\t", parts);
        }

        private IEnumerable<GraphLabel> ReadLabels(string text)
        {
            return text.Split('|').Select(p => new GraphLabel(p));
        }


        private string WriteLabels(IEnumerable<GraphLabel> labels)
        {
            return string.Join("|", labels);
        }
    }
}
