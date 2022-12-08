using System.Text.RegularExpressions;

namespace Repka.FileSystems
{
    public record FileSystemEntry(string Path, string? Origin = null)
    {
        public FileSystemEntry Replace(string pattern, string replacement)
        {
            string path = Regex.Replace(Path, pattern, replacement);
            return new FileSystemEntry(path, Origin);
        }

        public FileSystemEntry MakeAbsolute()
        {
            string path = Origin is not null
                ? System.IO.Path.GetFullPath(Path, System.IO.Path.GetDirectoryName(Origin) ?? "")
                : System.IO.Path.GetFullPath(Path);
            return new FileSystemEntry(path, Origin);
        }
    }
}
