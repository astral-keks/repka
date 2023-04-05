using System.Collections;
using System.Collections.Immutable;

namespace Repka.Graphs
{
    public class GraphTrace : IEnumerable<GraphLink>
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
    }
}
