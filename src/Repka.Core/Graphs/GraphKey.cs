using Repka.Strings;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Repka.Graphs
{
    public class GraphKey : Normalizable, IComparable<GraphKey>
    {
        public static readonly GraphKey Null = new("null");

        public static GraphKey Compose(params GraphKey[] keys)
        {
            return Compose(keys.Select(key => key.Resource).ToArray());
        }

        public static GraphKey Compose(params string[] resources)
        {
            return Compose(resources, 0) ?? throw new ArgumentException("Key could not be created");
        }

        private static GraphKey? Compose(string[] resources, int index)
        {
            GraphKey? key = null;

            if (index < resources.Length)
            { 
                string first = resources[index];
                GraphKey? second = Compose(resources, index + 1);
                key = second is not null
                    ? new GraphKey($"{first}+{second}")
                    : new GraphKey(first);
            }

            return key;
        }

        public static implicit operator string(GraphKey key) => key.Resource;
        public static implicit operator GraphKey(string resource) => new(resource);
        public GraphKey(string resource) : base(resource)
        {
            Resource = resource;
        }

        public string Resource { get; }

        public bool ContainsAny(params string[] texts)
        {
            return texts.Any(Contains);
        }

        public bool Contains(string text)
        {
            return Resource.Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        public bool Matches(string pattern, RegexOptions options = RegexOptions.IgnoreCase)
        {
            return Regex.IsMatch(Resource, pattern, options);
        }

        public static bool operator !=(GraphKey? first, GraphKey? second)
        {
            return !(first == second);
        }

        public static bool operator ==(GraphKey? first, GraphKey? second)
        {
            return ReferenceEquals(first, second) || first?.Equals(second) == true;
        }

        public int CompareTo(GraphKey? other)
        {
            return string.Compare(Normalized, other?.Normalized);
        }

        public override bool Equals(object? obj)
        {
            return obj is GraphKey key && 
                Equals(Normalized, key.Normalized);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Normalized);
        }

        public Guid GetGuid()
        {
            byte[] data = Encoding.UTF8.GetBytes(Normalized);

            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);

            return new Guid(hash);
        }

        public override string ToString()
        {
            return Resource;
        }
    }
}
