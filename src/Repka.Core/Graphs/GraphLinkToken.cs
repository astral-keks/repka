namespace Repka.Graphs
{
    public sealed class GraphLinkToken : GraphToken
    {
        public GraphLinkToken(GraphKey sourceKey, GraphKey targetKey, params GraphLabel[] labels)
            : base(new[] { sourceKey, targetKey, GraphKey.Compose(sourceKey, targetKey) }, labels)
        {
            SourceKey = sourceKey;
            TargetKey = targetKey;
        }

        public GraphKey SourceKey { get; }

        public GraphKey TargetKey { get; }
    }
}
