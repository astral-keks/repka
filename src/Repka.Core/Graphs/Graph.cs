namespace Repka.Graphs
{
    public class Graph
    {
        private readonly GraphDictionary<GraphKey, GraphNode> _nodes = new(node => node.Token.Keys);
        private readonly GraphDictionary<GraphKey, GraphLink> _links = new(link => link.Token.Keys);

        public void Add(GraphToken token)
        {
            if (token is GraphNodeToken nodeToken)
            {
                GraphState state = new();
                GraphNode node = new(nodeToken, state, this);
                _nodes.Add(node)?.Token.Label(nodeToken.Labels);
            }
            else if (token is GraphLinkToken linkToken)
            {
                GraphState state = new();
                GraphLink link = new(linkToken, state, this);
                _links.Add(link)?.Token.Label(linkToken.Labels);
            }
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
            ISet<GraphNode> nodeTokens = _nodes.FindAll(key);
            return nodeTokens.FirstOrDefault();
        }

        public IEnumerable<GraphNode> Nodes(params GraphLabel[] labels)
        {
            return _nodes.Where(node => node.Labels.ContainsAll(labels));
        }

        public GraphLink? Link(GraphKey sourceKey, GraphKey targetKey)
        {
            ISet<GraphLink> linkTokens = _links.FindAll(GraphKey.Compose(sourceKey, targetKey));
            return linkTokens.FirstOrDefault();
        }

        public IEnumerable<GraphLink> Links(GraphKey nodeKey, params GraphLabel[] labels)
        {
            return _links.FindAll(nodeKey).Where(link => link.Labels.ContainsAll(labels));
        }

        public IEnumerable<GraphLink> Links(params GraphLabel[] labels)
        {
            return _links.Where(link => link.Labels.ContainsAll(labels));
        }
    }
}
