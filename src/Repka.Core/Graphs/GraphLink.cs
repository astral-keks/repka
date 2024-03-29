﻿namespace Repka.Graphs
{
    public class GraphLink : GraphElement
    {
        private readonly GraphLinkToken _token;

        protected GraphLink(GraphLink link)
            : this(link._token, link.State, link.Graph)
        {
        }

        protected internal GraphLink(GraphLinkToken token, GraphState state, Graph graph)
            : base(token, state, graph)
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

        public IEnumerable<GraphNode> Nodes()
        {
            if (Source() is GraphNode source)
                yield return source;
            if (Target() is GraphNode target)
                yield return target;
        }
    }
}
