namespace Repka.Graphs
{
    public abstract class GraphProvider
    {
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
