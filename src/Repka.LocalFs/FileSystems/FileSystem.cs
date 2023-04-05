namespace Repka.FileSystems
{
    public sealed class FileSystem
    {
        private readonly Func<FileSystemEntry, IEnumerable<FileSystemEntry>> _factory;

        internal FileSystem(Func<FileSystemEntry, IEnumerable<FileSystemEntry>> factory)
        {
            _factory = factory;
        }

        public IEnumerable<FileSystemEntry> GetEntries(FileSystemEntry entry)
        {
            return _factory(entry);
        }
    }
}
