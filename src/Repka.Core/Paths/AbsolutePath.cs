using Repka.Strings;

namespace Repka.Paths
{
    public sealed class AbsolutePath : Normalized
    {
        public static implicit operator string(AbsolutePath path) => path.Original;
        public static implicit operator AbsolutePath(string path) => new(path);
        public AbsolutePath(string value)
            : base(new Uri(value).LocalPath)
        {
            if (!Path.IsPathRooted(Original))
                throw new ArgumentException($"Path {Original} is not absolute");
        }

        public bool Includes(RelativePath relativePath)
        {
            return Original.Contains(relativePath.Original, StringComparison.OrdinalIgnoreCase);
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
