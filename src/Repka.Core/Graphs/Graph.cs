using Repka.Optionals;

namespace Repka.Graphs
{
    public class Graph
    {
        private readonly GraphDictionary<GraphKey, GraphNodeToken> _nodes = new(nodeToken => nodeToken.Keys);
        private readonly GraphDictionary<GraphKey, GraphLinkToken> _links = new(linkToken => linkToken.Keys);
        private readonly GraphDictionary<GraphToken, GraphAttribute> _attributes = new(attribute => attribute.Referer);

        public void Add(GraphToken token)
        {
            if (token is GraphNodeToken nodeToken)
                _nodes.Add(nodeToken, token => token.Labels.AddRange(nodeToken.Labels));
            else if (token is GraphLinkToken linkToken)
                _links.Add(linkToken, token => token.Labels.AddRange(linkToken.Labels));
        }

        public bool Contains(GraphToken token)
        {
            if (token is GraphNodeToken nodeToken)
                return _nodes.Contains(nodeToken);
            else if (token is GraphLinkToken linkToken)
                return _links.Contains(linkToken);
            return false;
        }

        public void Set(GraphAttribute attribute)
        {
            _attributes.Add(attribute);
        }

        public IEnumerable<GraphAttribute> Attributes(GraphToken referer)
        {
            return _attributes.Get(referer);
        }

        public GraphElement? Element(GraphToken token)
        {
            GraphElement? element = default;

            if (token is GraphNodeToken nodeToken)
                element = Node(nodeToken.Key);
            else if (token is GraphLinkToken linkToken)
                element = Link(linkToken.SourceKey, linkToken.TargetKey);

            return element;
        }

        public GraphNode? Node(GraphKey key)
        {
            ISet<GraphNodeToken> nodeTokens = _nodes.Get(key);
            return nodeTokens.FirstOrDefault().ToOptional()
                .Map(nodeToken => new GraphNode(nodeToken, this))
                .OrElseDefault();
        }

        public IEnumerable<GraphNode> Nodes(params GraphLabel[] labels)
        {
            return _nodes
                .Select(nodeToken => new GraphNode(nodeToken, this))
                .Where(node => node.Labels.Any(labels));
        }

        public GraphLink? Link(GraphKey sourceKey, GraphKey targetKey)
        {
            ISet<GraphLinkToken> linkTokens = _links.Get(GraphKey.Compose(sourceKey, targetKey));
            return linkTokens.FirstOrDefault().ToOptional()
                .Map(linkToken => new GraphLink(linkToken, this))
                .OrElseDefault();
        }

        public IEnumerable<GraphLink> Links(GraphKey nodeKey, params GraphLabel[] labels)
        {
            return _links.Get(nodeKey)
                .Select(linkToken => new GraphLink(linkToken, this))
                .Where(link => link.Labels.Any(labels));
        }

        public IEnumerable<GraphLink> Links(params GraphLabel[] labels)
        {
            return _links
                .Select(linkToken => new GraphLink(linkToken, this))
                .Where(link => link.Labels.Any(labels));
        }
    }
}
