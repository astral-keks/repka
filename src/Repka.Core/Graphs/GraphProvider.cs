
namespace Repka.Graphs
{
    public abstract class GraphProvider
    {
        public GraphProgress Progress { protected get; init; } = new();

        public virtual void AddTokens(GraphKey key, Graph graph)
        {
            foreach (var token in GetTokens(key, graph))
            {
                graph.Add(token);
                foreach (var attribute in GetAttributes(token, graph))
                {
                    graph.Set(attribute);
                }
            }
        }

        public virtual IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            yield break;
        }

        public virtual IEnumerable<GraphAttribute> GetAttributes(GraphToken token, Graph graph)
        {
            yield break;
        }
    }
}
