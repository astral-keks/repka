using Repka.Diagnostics;

namespace Repka.Graphs
{
    public abstract class GraphProvider
    {
        public Progress Progress { protected get; init; } = ProgressTextual.Console;

        public virtual void AddTokens(GraphKey key, Graph graph)
        {
        }
    }
}
