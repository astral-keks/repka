using Repka.Collections;

namespace Repka.Graphs
{
    public class Graph
    {
        private readonly GraphDictionary<GraphKey, GraphNodeToken> _nodes = new(nodeToken => nodeToken.Keys);
        private readonly GraphDictionary<GraphKey, GraphLinkToken> _links = new(linkToken => linkToken.Keys);
        private readonly GraphDictionary<GraphKey, GraphAttribute> _attributes = new(attribute => attribute.Key);

        public void Add(GraphToken token)
        {
            if (token is GraphNodeToken nodeToken)
            {
                _nodes.Add(nodeToken);
                _nodes.Find(nodeToken)?.Label(nodeToken.Labels);
            }
            else if (token is GraphLinkToken linkToken)
            {
                _links.Add(linkToken);
                _links.Find(linkToken)?.Label(linkToken.Labels);
            }
        }

        public void Add(GraphAttribute attribute)
        {
            _attributes.Add(attribute);
        }

        public bool Contains(GraphToken token)
        {
            if (token is GraphNodeToken nodeToken)
                return _nodes.Contains(nodeToken);
            else if (token is GraphLinkToken linkToken)
                return _links.Contains(linkToken);
            return false;
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
            ISet<GraphNodeToken> nodeTokens = _nodes.FindAll(key);
            return nodeTokens.FirstOrDefault().ToOptional()
                .Map(nodeToken => new GraphNode(nodeToken, this))
                .OrElseDefault();
        }

        public IEnumerable<GraphNode> Nodes(params GraphLabel[] labels)
        {
            return _nodes
                .Select(nodeToken => new GraphNode(nodeToken, this))
                .Where(node => node.Labels.ContainsAll(labels));
        }

        public GraphLink? Link(GraphKey sourceKey, GraphKey targetKey)
        {
            ISet<GraphLinkToken> linkTokens = _links.FindAll(GraphKey.Compose(sourceKey, targetKey));
            return linkTokens.FirstOrDefault().ToOptional()
                .Map(linkToken => new GraphLink(linkToken, this))
                .OrElseDefault();
        }

        public IEnumerable<GraphLink> Links(GraphKey nodeKey, params GraphLabel[] labels)
        {
            return _links.FindAll(nodeKey)
                .Select(linkToken => new GraphLink(linkToken, this))
                .Where(link => link.Labels.ContainsAll(labels));
        }

        public IEnumerable<GraphLink> Links(params GraphLabel[] labels)
        {
            return _links
                .Select(linkToken => new GraphLink(linkToken, this))
                .Where(link => link.Labels.ContainsAll(labels));
        }

        public IEnumerable<GraphAttribute> Attributes(GraphKey key)
        {
            return _attributes.FindAll(key);
        }
    }
}
