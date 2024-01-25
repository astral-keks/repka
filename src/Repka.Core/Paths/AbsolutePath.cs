using Repka.Strings;

namespace Repka.Paths
{
    public sealed class AbsolutePath : Normalized
    {
        public static AbsolutePath Create(IEnumerable<string> segments) => string.Join(Path.DirectorySeparatorChar, segments);
        public static implicit operator string(AbsolutePath path) => path.Original;
        public static implicit operator AbsolutePath(string path) => new(path);
        public AbsolutePath(string value)
            : base(new Uri(value).LocalPath)
        {
            if (!Path.IsPathRooted(Original))
                throw new ArgumentException($"Path {Original} is not absolute");
        }

        public string Name => Path.GetFileName(Original); 

        public bool Includes(RelativePath relativePath)
        {
            return Original.Contains(relativePath.Original, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<AbsolutePath> Parents()
        {
            AbsolutePath? path = Parent();
            while (path is not null)
            {
                yield return path;
                path = path.Parent();
            }
        }

        public AbsolutePath? Parent()
        {
            string? directory = Path.GetDirectoryName(Original);
            return directory is not null ? new(directory) : default;
        }

        public AbsolutePath Combine(string path)
        {
            return new AbsolutePath(Path.Combine(Original, path));
        }
    }
}
