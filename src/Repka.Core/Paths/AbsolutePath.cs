using Repka.Strings;

namespace Repka.Paths
{
    public sealed class AbsolutePath : Normalized
    {
        public static implicit operator string(AbsolutePath path) => path.Original;
        public static implicit operator AbsolutePath(string path) => new(path);
        public AbsolutePath(string value)
            : base(value)
        {
            if (!Path.IsPathRooted(Original))
                throw new ArgumentException($"Path {Original} is not absolute");
        }
    }
}
