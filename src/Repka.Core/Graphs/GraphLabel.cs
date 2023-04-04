namespace Repka.Graphs
{
    public readonly struct GraphLabel
    {
        public static implicit operator string(GraphLabel label) => label.Value;
        public static implicit operator GraphLabel(string? value) => new(value);
        public GraphLabel(string? value)
        {
            Value = value ?? string.Empty;
        }

        public readonly string Value { get; }

        public override string ToString()
        {
            return Value;
        }
    }

    public static class GraphLabelExtensions
    {
        public static void AddRange(this ICollection<GraphLabel> source, IEnumerable<GraphLabel> labels)
        {
            foreach (var label in labels)
                source.Add(label);
        }

        public static bool All(this IEnumerable<GraphLabel> source, IEnumerable<GraphLabel> labels)
        {
            return !labels.Any() || labels.All(source.Contains);
        }

        public static bool Any(this IEnumerable<GraphLabel> source, IEnumerable<GraphLabel> labels)
        {
            return !labels.Any() || labels.Any(source.Contains);
        }
    }
}
