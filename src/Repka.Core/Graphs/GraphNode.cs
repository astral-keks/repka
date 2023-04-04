using Repka.Diagnostics;
using System.Xml.Linq;
using System;
using System.Text;

namespace Repka.Graphs
{
    public class GraphNode : GraphElement
    {
        private readonly GraphNodeToken _token;

        protected GraphNode(GraphNode node)
            : this(node._token, node.Graph)
        {
        }

        protected internal GraphNode(GraphNodeToken token, Graph graph)
            : base(token, graph)
        {
            _token = token;
        }

        public GraphKey Key => _token.Key;

        public IEnumerable<GraphNode> Siblings(GraphLabel subject)
        {
            return Subjects(subject).SingleOrDefault()?.Objects() ?? Enumerable.Empty<GraphNode>();
        }

        public IEnumerable<GraphNode> Neighbors(params GraphLabel[] labels)
        {
            return Enumerable.Union(Subjects(labels), Objects(labels));
        }

        public IEnumerable<GraphNode> Subjects(params GraphLabel[] labels)
        {
            return Inputs()
                .Select(output => output.Source())
                .OfType<GraphNode>()
                .Where(obj => obj.Labels.All(labels));
        }

        public IEnumerable<GraphNode> Objects(params GraphLabel[] labels)
        {
            return Outputs()
                .Select(output => output.Target())
                .OfType<GraphNode>()
                .Where(obj => obj.Labels.All(labels));
        }

        public IEnumerable<GraphLink> Links(params GraphLabel[] labels)
        {
            return Enumerable.Union(Inputs(labels), Outputs(labels));
        }

        public IEnumerable<GraphLink> Inputs(params GraphLabel[] labels)
        {
            return Graph.Links(Key, labels)
                .Where(link => link.TargetKey == Key);
        }

        public IEnumerable<GraphLink> Outputs(params GraphLabel[] labels)
        {
            return Graph.Links(Key, labels)
                .Where(link => link.SourceKey == Key);
        }
    }
}
