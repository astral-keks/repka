using Repka.FileSystems;

namespace Repka.Caching
{
    public class FileSystemCacheProvider : CacheProvider
    {
        protected override List<CacheEntry> Read(string store, string name)
        {
            List<CacheEntry> entries = new(0);

            string directory = FileSystemPaths.Aux(store);
            Directory.CreateDirectory(directory);

            string location = Path.Combine(directory, $"{name}.txt");
            if (File.Exists(location))
            {
                using StreamReader reader = new(location);
                entries.AddRange(reader.ReadEntries());
            }

            return entries;
        }

        protected override void Write(string store, string name, IEnumerable<CacheEntry> entries)
        {
            string directory = FileSystemPaths.Aux(store);
            Directory.CreateDirectory(directory);

            string location = Path.Combine(directory, $"{name}.txt");
            using StreamWriter writer = new(location, append: false);
            writer.WriteEntries(entries);
        }
    }
}
