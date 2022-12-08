namespace Repka.FileSystems
{
    public static class FileSystemExtensions
    {
        public static FileSystem Map(this FileSystem source, Func<FileSystemEntry, FileSystemEntry?> mapper)
        {
            IEnumerable<FileSystemEntry> map(FileSystemEntry input)
            {
                foreach (var entry in source.GetEntries(input))
                {
                    FileSystemEntry? result = mapper(entry);
                    if (result is not null)
                        yield return new FileSystemEntry(result.Path, entry.Path);
                }
            }

            return new(map);
        }

        public static FileSystem Then(this FileSystem source, FileSystem target)
        {
            IEnumerable<FileSystemEntry> then(FileSystemEntry input)
            {
                foreach (var entry in source.GetEntries(input))
                {
                    yield return entry;

                    foreach (var next in target.GetEntries(entry))
                        yield return new FileSystemEntry(next.Path, entry.Path);
                }
            }

            return new(then);
        }

        public static FileSystem Pipe(this FileSystem source, FileSystem target)
        {
            IEnumerable<FileSystemEntry> then(FileSystemEntry input)
            {
                foreach (var entry in source.GetEntries(input))
                {
                    foreach (var next in target.GetEntries(entry))
                        yield return new FileSystemEntry(next.Path, entry.Path);
                }
            }

            return new(then);
        }
    }
}
