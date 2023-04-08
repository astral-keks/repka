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

        public FileSystem Map(Func<FileSystemEntry, FileSystemEntry?> mapper)
        {
            IEnumerable<FileSystemEntry> map(FileSystemEntry input)
            {
                foreach (var entry in GetEntries(input))
                {
                    FileSystemEntry? result = mapper(entry);
                    if (result is not null)
                        yield return new FileSystemEntry(result.Path, entry.Path);
                }
            }

            return new(map);
        }

        public FileSystem Then(FileSystem target)
        {
            IEnumerable<FileSystemEntry> then(FileSystemEntry input)
            {
                foreach (var entry in GetEntries(input))
                {
                    yield return entry;

                    foreach (var next in target.GetEntries(entry))
                        yield return new FileSystemEntry(next.Path, entry.Path);
                }
            }

            return new(then);
        }

        public FileSystem Pipe(FileSystem target)
        {
            IEnumerable<FileSystemEntry> then(FileSystemEntry input)
            {
                foreach (var entry in GetEntries(input))
                {
                    foreach (var next in target.GetEntries(entry))
                        yield return new FileSystemEntry(next.Path, entry.Path);
                }
            }

            return new(then);
        }
    }
}
