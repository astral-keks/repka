using System.Xml.Linq;

namespace Repka.FileSystems
{
    public static class FileSystemDefinitions
    {
        public static FileSystem Files(string wildcard, EnumerationOptions? options = default)
        {
            options ??= new() { RecurseSubdirectories = true };

            IEnumerable<FileSystemEntry> files(FileSystemEntry input)
            {
                if (Directory.Exists(input.Path))
                {
                    foreach (var path in Directory.EnumerateFiles(input.Path, wildcard, options))
                    {
                        yield return new FileSystemEntry(path);
                    }
                }
            }

            return new(files);
        }

        public static FileSystem Content()
        {
            IEnumerable<FileSystemEntry> content(FileSystemEntry input)
            {
                if (File.Exists(input.Path))
                {
                    foreach (var path in File.ReadAllLines(input.Path))
                    {
                        if (path is not null)
                            yield return new FileSystemEntry(path, input.Path);
                    }
                }
            }

            return new(content);
        }

        public static FileSystem Concat(params FileSystem[] elements)
        {
            IEnumerable<FileSystemEntry> concat(FileSystemEntry input)
            {
                return elements.SelectMany(element => element.GetEntries(input));
            }

            return new(concat);
        }

        public static FileSystem Empty()
        {
            return new(entry => Enumerable.Empty<FileSystemEntry>());
        }
    }
}
