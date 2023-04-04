using System.Collections;

namespace Repka.Graphs
{
    public static class GraphFragment
    {
        public static GraphFragment<TElement> ToFragment<TElement>(this IEnumerable<TElement> source, Func<TElement, IEnumerable<TElement>> flatten)
            where TElement : GraphElement
        {
            return new GraphFragment<TElement>(source.ToList(), flatten);
        }
    }

    public sealed class GraphFragment<TElement> : IEnumerable<TElement>
        where TElement : GraphElement
    {
        private readonly ICollection<TElement> _roots;
        private readonly Func<TElement, IEnumerable<TElement>> _flatten;

        public GraphFragment(ICollection<TElement> roots, Func<TElement, IEnumerable<TElement>> flatten)
        {
            _roots = roots;
            _flatten = flatten;
        }

        public IEnumerable<TElement> Traverse()
        {
            GraphTraversal<TElement> traversal = new() { Strategy = GraphTraversalStrategy.BypassHistory };
            return _roots.Traverse(_flatten, traversal);
        }

        public IEnumerable<TElement> Flatten()
        {
            GraphTraversal<TElement> traversal = new() { Strategy = GraphTraversalStrategy.RecallHistory };
            return _roots.Traverse(_flatten, traversal);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _roots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
