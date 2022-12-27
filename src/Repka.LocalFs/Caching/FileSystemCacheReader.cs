namespace Repka.Caching
{
    internal static class FileSystemCacheReader
    {
        public static IEnumerable<CacheEntry> ReadEntries(this StreamReader reader)
        {
            CacheEntry.Builder? builder = null;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine() ?? string.Empty;
                if (line == FileSystemCacheProtocol.EntryElement)
                {
                    CacheEntry? entry = reader.ReadEntry(ref builder);
                    if (entry is not null)
                        yield return entry;
                }
                else if (line == FileSystemCacheProtocol.PropertyElement && builder is not null)
                {
                    reader.ReadProperty(builder);
                }
                else if (line == FileSystemCacheProtocol.BeginContentElement && builder is not null)
                {
                    reader.ReadContent(builder, line => line == FileSystemCacheProtocol.EndContentElement);
                }
            }

            if (builder is not null)
                yield return builder.Build();
        }

        private static CacheEntry? ReadEntry(this StreamReader reader, ref CacheEntry.Builder? builder)
        {
            CacheEntry? entry = null;

            if (builder is not null)
                entry = builder.Build();

            string? key = reader.ReadLine();
            if (key is not null)
            {
                builder = new CacheEntry.Builder
                {
                    Key = key
                };
            }

            return entry;
        }

        private static void ReadProperty(this StreamReader reader, CacheEntry.Builder builder)
        {
            string? name = reader.ReadLine();
            string? value = reader.ReadLine();
            if (name is not null && value is not null)
                builder?.Properties.Add(new CacheProperty(name, value));
        }

        private static void ReadContent(this StreamReader reader, CacheEntry.Builder builder, Func<string, bool> until)
        {
            List<string> lines = new();
            string? line;
            while((line = reader.ReadLine()) is not null && !until(line))
            {
                lines.Add(line);
            }

            builder.Content = new(lines);
        }
    }
}
