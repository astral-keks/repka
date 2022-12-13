namespace Repka.Caching
{
    public class FileStorage : ObjectStorage
    {
        public string Root { get; init; } = Directory.GetCurrentDirectory();

        public override Stream Read(string key)
        {
            FileStorageEntry entry = new(key);
            string path = entry.Path(Root);
            
            return File.Exists(path) ? File.OpenRead(path) : Stream.Null;
        }

        public override Stream Write(string key)
        {
            if (!Directory.Exists(Root))
                Directory.CreateDirectory(Root);

            FileStorageEntry entry = new(key);
            string path = entry.Path(Root);

            return File.OpenWrite(path);
        }
    }
}
