using Repka.Caching;

namespace Repka.Graphs
{
    public class GraphFactory
    {
        private readonly IEnumerable<GraphProvider> _providers;

        public GraphFactory(params GraphProvider[] providers)
        {
            _providers = providers;
        }

        public Graph CreateGraph(GraphKey key)
        {
            Graph graph = new();

            foreach (var provider in _providers)
            {
                provider.AddTokens(key, graph);
            }

            return graph;
        }
    }
}
