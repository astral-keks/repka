using Repka.Collections;
using System.Collections;
using System.Collections.Immutable;

namespace Repka.Graphs
{
    public class GraphTrace : IEnumerable<GraphLink>, IComparable<GraphTrace>
    {
        private readonly ImmutableList<GraphLink> _reversedLinks;

        public static GraphTrace Single(GraphLink link) => 
            new(ImmutableList.Create(link));

        public GraphTrace(ImmutableList<GraphLink>? links = default)
        {
            _reversedLinks = links ?? ImmutableList.Create<GraphLink>();
        }

        public int Length => _reversedLinks.Count;

        public GraphNode? Source => _reversedLinks.Last().Source();

        public GraphNode? Target => _reversedLinks.First().Target();

        public GraphTrace Prepend(GraphLink link) => new(_reversedLinks.Add(link));

        public IEnumerator<GraphLink> GetEnumerator()
        {
            return _reversedLinks.Reverse().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int CompareTo(GraphTrace? other)
        {
            return Length.CompareTo(other?.Length ?? 0);
        }
    }

    public static class GraphTracing
    {
        public static IEnumerable<GraphTrace> TraceForward(this GraphNode sourceNode, GraphNode traceNode,
            Func<IEnumerable<GraphLink>, IEnumerable<GraphLink>>? linkSelector = default,
            Func<IEnumerable<GraphNode>, IEnumerable<GraphNode>>? nodeSelector = default)
        {
            Inspection<GraphNode, GraphTrace> inspection = new();
            linkSelector ??= links => links; nodeSelector ??= nodes => nodes;

            return sourceNode.Trace(traceNode, node => linkSelector(node.Outputs()), link => nodeSelector(link.Target().ToOptional()), inspection);
        }

        public static IEnumerable<GraphTrace> TraceBackward(this GraphNode sourceNode, GraphNode traceNode,
            Func<IEnumerable<GraphLink>, IEnumerable<GraphLink>>? linkSelector = default,
            Func<IEnumerable<GraphNode>, IEnumerable<GraphNode>>? nodeSelector = default)
        {
            Inspection<GraphNode, GraphTrace> inspection = new();
            linkSelector ??= links => links; nodeSelector ??= nodes => nodes;
            return sourceNode.Trace(traceNode, node => linkSelector(node.Inputs()), link => nodeSelector(link.Source().ToOptional()), inspection);
        }

        private static IEnumerable<GraphTrace> Trace(this GraphNode sourceNode, GraphNode traceNode,
            Func<GraphNode, IEnumerable<GraphLink>> linkSelector, Func<GraphLink, IEnumerable<GraphNode>> nodeSelector,
            Inspection<GraphNode, GraphTrace> inspection)
        {
            return inspection.InspectOrGet(sourceNode, () => linkSelector(sourceNode)
                .Distinct()
                .SelectMany(link => link.Trace(traceNode, linkSelector, nodeSelector, inspection))
                .Where(trace => trace.Length > 0)
                .ToList());
        }

        private static IEnumerable<GraphTrace> Trace(this GraphLink sourceLink, GraphNode traceNode,
            Func<GraphNode, IEnumerable<GraphLink>> linkSelector, Func<GraphLink, IEnumerable<GraphNode>> nodeSelector,
            Inspection<GraphNode, GraphTrace> inspection)
        {
            foreach (var sourceNode in nodeSelector(sourceLink))
            {
                if (sourceNode == traceNode)
                {
                    yield return GraphTrace.Single(sourceLink);
                }
                else
                {
                    foreach (var trace in sourceNode.Trace(traceNode, linkSelector, nodeSelector, inspection))
                    {
                        yield return trace.Prepend(sourceLink);
                    }
                }
            }
        }
    }
}
