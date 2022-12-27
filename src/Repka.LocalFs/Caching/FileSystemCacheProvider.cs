namespace Repka.Caching
{
    public class FileSystemCacheProvider : CacheProvider
    {
        public string Root { get; init; } = Directory.GetCurrentDirectory();

        protected override List<CacheEntry> Read(string cacheName)
        {
            List<CacheEntry> entries = new(0);

            FileSystemCacheLocation location = new(cacheName);
            string path = location.FullName(Root);

            if (File.Exists(path))
            {
                using StreamReader reader = new(path);
                entries.AddRange(reader.ReadEntries());
            }

            return entries;
        }

        protected override void Write(string cacheName, IEnumerable<CacheEntry> entries)
        {
            FileSystemCacheLocation location = new(cacheName);
            string path = location.FullName(Root);

            string? directory = Path.GetDirectoryName(path);
            if (directory is not null)
                Directory.CreateDirectory(directory);

            using StreamWriter writer = new(path, append: false);
            writer.WriteEntries(entries);
        }
    }
}
