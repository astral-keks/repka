using Repka.FileSystems;

namespace Repka.Caching
{
    public class FileSystemCacheProvider : CacheProvider
    {
        private readonly string _root;

        public FileSystemCacheProvider(string root)
        {
            _root = root;
        }

        protected override List<CacheEntry> Read(string store, string name)
        {
            List<CacheEntry> entries = new(0);

            Directory.CreateDirectory(_root);

            string location = Path.Combine(_root, $"{name}.txt");
            if (File.Exists(location))
            {
                using StreamReader reader = new(location);
                entries.AddRange(reader.ReadEntries());
            }

            return entries;
        }

        protected override void Write(string store, string name, IEnumerable<CacheEntry> entries)
        {
            Directory.CreateDirectory(_root);

            string location = Path.Combine(_root, $"{name}.txt");
            using StreamWriter writer = new(location, append: false);
            writer.WriteEntries(entries);
        }
    }
}
