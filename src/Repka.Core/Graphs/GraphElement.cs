using System.Text;

namespace Repka.Graphs
{
    public abstract class GraphElement
    {
        internal GraphElement(GraphToken token, Graph graph)
        {
            Token = token;
            Graph = graph;
        }

        public IEnumerable<GraphLabel> Labels => Token.Labels;

        public GraphToken Token { get; }

        public Graph Graph { get; }

        public string Text()
        {
            StringBuilder text = new();
            foreach (var key in Token.Keys)
                text.Append($" {key}");
            foreach (var label in Token.Labels)
                text.Append($" {label}");
            return text.ToString();
        }

        public GraphAttribute<TValue>? Attribute<TValue>()
        {
            return Graph.Attributes(Token)
                .OfType<GraphAttribute<TValue>>()
                .SingleOrDefault();
        }

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
