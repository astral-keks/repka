namespace Repka.Graphs
{
    public sealed class GraphNodeToken : GraphToken, IComparable<GraphNodeToken>
    {
        public GraphNodeToken(GraphKey key, params GraphLabel[] labels)
            : base(new[] { key }, labels)
        {
            Key = key;
        }

        public GraphKey Key { get; }

        public int CompareTo(GraphNodeToken? other)
        {
            return Key.CompareTo(other?.Key);
        }

        public override string ToString()
        {
            return $"Key={Key}; Labels={string.Join(", ", Labels)}";
        }
    }
}
