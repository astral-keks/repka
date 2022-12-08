namespace Repka.Graphs
{
    public class GraphFactory
    {
        private readonly List<GraphProvider> _providers;

        public GraphFactory(params GraphProvider[] providers)
            : this(providers.AsEnumerable())
        {
        }

        public GraphFactory(IEnumerable<GraphProvider> providers)
        {
            _providers = providers.ToList();
        }

        public Graph CreateGraph(params GraphKey[] keys)
        {
            Graph graph = new();

            foreach (var provider in _providers)
            {
                foreach (var key in keys)
                {
                    foreach (var token in provider.GetTokens(key, graph))
                    {
                        graph.Add(token);
                        foreach (var attribute in provider.GetAttributes(token, graph))
                        {
                            graph.Set(attribute);
                        }
                    }
                }
            }

            return graph;
        }
    }
}
