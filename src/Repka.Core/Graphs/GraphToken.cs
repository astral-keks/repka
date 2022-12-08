namespace Repka.Graphs
{
    public abstract class GraphToken
    {
        internal GraphToken(IReadOnlyList<GraphKey> keys, IList<GraphLabel> labels)
        {
            Keys = keys.ToHashSet();
            Labels = labels.ToHashSet();
        }

        public IReadOnlySet<GraphKey> Keys { get; }

        public ISet<GraphLabel> Labels { get; }

        public override bool Equals(object? obj)
        {
            return obj is GraphToken token && Keys.SequenceEqual(token.Keys);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            foreach (var key in Keys)
                hashCode.Add(key);
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            return string.Join(" : ", Keys);
        }
    }
}
