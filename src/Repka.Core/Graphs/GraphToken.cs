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

        public GraphToken Label(params GraphLabel[] labels) => Label(labels.AsEnumerable());
        public GraphToken Label(IEnumerable<string> labels) => Label(labels.Select(label => new GraphLabel(label)));
        public GraphToken Label(IEnumerable<GraphLabel> labels)
        {
            foreach (var label in labels)
                Labels.Add(label);
            return this;
        }

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
            return $"Keys={string.Join(", ", Keys)}";
        }
    }
}
