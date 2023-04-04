using Repka.Diagnostics;

namespace Repka.Graphs
{
    public abstract class GraphProvider
    {
        public Progress Progress { protected get; init; } = ProgressTextual.Console;

        public void AddTokens(GraphKey key, Graph graph)
        {
            foreach (var token in GetTokens(key, graph))
            {
                graph.Add(token);
            }
        }

        public virtual IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            yield break;
        }
    }
}
