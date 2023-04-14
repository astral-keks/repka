using Repka.Strings;

namespace Repka.Paths
{
    public class RelativePath : Normalized
    {
        public static implicit operator string(RelativePath path) => path.Original;
        public static implicit operator RelativePath(string path) => new(path);
        public RelativePath(string value)
            : base(value)
        {
        }
    }
}
