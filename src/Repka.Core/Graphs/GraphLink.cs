
namespace Repka.Graphs
{
    public sealed class GraphLink : GraphElement
    {
        private readonly GraphLinkToken _token;

        internal GraphLink(GraphLinkToken token, Graph graph)
            : base(token, graph)
        {
            _token = token;
        }

        public GraphKey SourceKey => _token.SourceKey;

        public GraphNode? Source()
        {
            return Graph.Node(_token.SourceKey);
        }

        public GraphKey TargetKey => _token.TargetKey;

        public GraphNode? Target()
        {
            return Graph.Node(_token.TargetKey);
        }
    }
}
