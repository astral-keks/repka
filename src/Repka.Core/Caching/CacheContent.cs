namespace Repka.Caching
{
    public class CacheContent
    {
        public CacheContent()
            : this(new List<string>(0))
        {
        }

        public CacheContent(List<string> lines)
        {
            Lines = lines;
        }

        public IReadOnlyList<string> Lines { get; }
    }
}
