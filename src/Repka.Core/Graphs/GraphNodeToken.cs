namespace Repka.Graphs
{
    public sealed class GraphNodeToken : GraphToken
    {
        public GraphNodeToken(GraphKey key, params GraphLabel[] labels)
            : base(new[] { key }, labels)
        {
            Key = key;
        }

        public GraphKey Key { get; }

        public override string ToString()
        {
            return $"Key={Key}; Labels={string.Join(", ", Labels)}";
        }
    }
}
