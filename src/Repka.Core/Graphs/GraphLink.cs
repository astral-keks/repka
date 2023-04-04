
namespace Repka.Graphs
{
    public class GraphLink : GraphElement
    {
        private readonly GraphLinkToken _token;

        protected GraphLink(GraphLink link)
            : this(link._token, link.Graph)
        {
        }

        protected internal GraphLink(GraphLinkToken token, Graph graph)
            : base(token, graph)
        {
            _token = token;
        }

        public GraphKey SourceKey => _token.SourceKey;

        public GraphNode? Source()
        {
            return Graph.Node(SourceKey);
        }

        public GraphKey TargetKey => _token.TargetKey;

        public GraphNode? Target()
        {
            return Graph.Node(TargetKey);
        }
    }
}
