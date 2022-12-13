using Repka.Caching;

namespace Repka.Graphs
{
    public class GraphFactory
    {
        private readonly IEnumerable<GraphProvider> _providers;
        private readonly ObjectCache? _cache;

        public GraphFactory(ObjectCache? cache, params GraphProvider[] providers)
        {
            _providers = providers;
            _cache = cache;
        }

        public Graph CreateGraph(GraphKey key)
        {
            Graph createGraph()
            {
                Graph graph = new();

                foreach (var provider in _providers)
                {
                    provider.AddTokens(key, graph);
                }

                return graph;
            }

            return _cache is not null
                ? _cache.GetOrAdd(key, createGraph)
                : createGraph();
        }
    }
}
