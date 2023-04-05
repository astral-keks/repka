namespace Repka.Graphs
{
    public abstract class GraphElement
    {
        protected internal GraphElement(GraphToken token, Graph graph)
        {
            Token = token;
            Graph = graph;
        }

        public IEnumerable<GraphLabel> Labels => Token.Labels;

        public GraphToken Token { get; }

        public Graph Graph { get; }

        public GraphAttribute<TValue>? Attribute<TValue>()
        {
            return Graph.Attributes(Token)
                .OfType<GraphAttribute<TValue>>()
                .SingleOrDefault();
        }

        public static bool operator ==(GraphElement? left, GraphElement? right) => Equals(left, right);

        public static bool operator !=(GraphElement? left, GraphElement? right) => !Equals(left, right);

        public override bool Equals(object? obj)
        {
            return obj is GraphElement element &&
                EqualityComparer<GraphToken>.Default.Equals(Token, element.Token);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Token);
        }

        public override string ToString()
        {
            return Token.ToString();
        }
    }
}
