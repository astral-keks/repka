namespace Repka.Graphs
{
    public abstract class GraphElement
    {
        protected internal GraphElement(GraphToken token, GraphState state, Graph graph)
        {
            Token = token;
            State = state;
            Graph = graph;
        }

        public GraphToken Token { get; }

        public GraphState State { get; }

        public Graph Graph { get; }

        public IEnumerable<GraphLabel> Labels => Token.Labels;

        public GraphAttribute Attribute(string name) => State.Attribute(name);

        public static bool operator ==(GraphElement? left, GraphElement? right) => Equals(left, right);

        public static bool operator !=(GraphElement? left, GraphElement? right) => !Equals(left, right);

        public override bool Equals(object? obj)
        {
            return obj is GraphElement element &&
                Equals(Token, element.Token);
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
