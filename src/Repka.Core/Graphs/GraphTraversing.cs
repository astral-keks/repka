using Repka.Collections;

namespace Repka.Graphs
{
    public static class GraphTraversing
    {
        public static IEnumerable<GraphTrace> TraceForward(this GraphNode sourceNode, GraphNode traceNode,
            Func<IEnumerable<GraphLink>, IEnumerable<GraphLink>>? linkSelector = default,
            Func<IEnumerable<GraphNode>, IEnumerable<GraphNode>>? nodeSelector = default)
        {
            GraphTraversal<GraphNode, GraphTrace> traversal = new();
            linkSelector ??= links => links; nodeSelector ??= nodes => nodes;

            return sourceNode.Trace(traceNode, node => linkSelector(node.Outputs()), link => nodeSelector(link.Target().ToOptional()), traversal);
        }

        public static IEnumerable<GraphTrace> TraceBackward(this GraphNode sourceNode, GraphNode traceNode,
            Func<IEnumerable<GraphLink>, IEnumerable<GraphLink>>? linkSelector = default,
            Func<IEnumerable<GraphNode>, IEnumerable<GraphNode>>? nodeSelector = default)
        {
            GraphTraversal<GraphNode, GraphTrace> traversal = new();
            linkSelector ??= links => links; nodeSelector ??= nodes => nodes;
            return sourceNode.Trace(traceNode, node => linkSelector(node.Inputs()), link => nodeSelector(link.Source().ToOptional()), traversal);
        }

        private static IEnumerable<GraphTrace> Trace(this GraphNode sourceNode, GraphNode traceNode,
            Func<GraphNode, IEnumerable<GraphLink>> linkSelector, Func<GraphLink, IEnumerable<GraphNode>> nodeSelector,
            GraphTraversal<GraphNode, GraphTrace> traversal)
        {
            return traversal.Visit(sourceNode, () => linkSelector(sourceNode)
                .Distinct()
                .SelectMany(link => link.Trace(traceNode, linkSelector, nodeSelector, traversal))
                .Where(trace => trace.Length > 0)
                .ToList());
        }

        private static IEnumerable<GraphTrace> Trace(this GraphLink sourceLink, GraphNode traceNode,
            Func<GraphNode, IEnumerable<GraphLink>> linkSelector, Func<GraphLink, IEnumerable<GraphNode>> nodeSelector,
            GraphTraversal<GraphNode, GraphTrace> traversal)
        {
            foreach (var sourceNode in nodeSelector(sourceLink))
            {
                if (sourceNode == traceNode)
                {
                    yield return GraphTrace.Single(sourceLink);
                }
                else
                {
                    foreach (var trace in sourceNode.Trace(traceNode, linkSelector, nodeSelector, traversal))
                    {
                        yield return trace.Prepend(sourceLink);
                    }
                }
            }
        }

        public static IEnumerable<TElement> Flatten<TElement>(this IEnumerable<TElement> elements, Func<TElement, IEnumerable<TElement>> expand,
            GraphTraversal<TElement>? traversal = default)
            where TElement : GraphElement
        {
            traversal ??= new();
            return elements.SelectMany(element => element.Traverse(expand, traversal));
        }

        public static ICollection<TElement> Traverse<TElement>(this TElement element, Func<TElement, IEnumerable<TElement>> expand,
            GraphTraversal<TElement>? traversal = default)
            where TElement : GraphElement
        {
            traversal ??= new();
            return traversal.Visit(element, () => expand(element).Flatten(expand, traversal).Prepend(element).ToList());
        }

    }
}
