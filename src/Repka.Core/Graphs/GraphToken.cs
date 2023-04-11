namespace Repka.Graphs
{
    public abstract class GraphToken
    {
        private readonly HashSet<GraphKey> _keys;
        private readonly HashSet<GraphLabel> _labels;

        internal GraphToken(IReadOnlyList<GraphKey> keys, IList<GraphLabel> labels)
        {
            _keys = keys.ToHashSet();
            _labels = labels.ToHashSet();
        }

        public IReadOnlySet<GraphKey> Keys => _keys;
        public IEnumerable<GraphLabel> Labels => _labels;
        
        public GraphToken Label(params GraphLabel[] labels) => Label(labels.AsEnumerable());
        public GraphToken Label(IEnumerable<string> labels) => Label(labels.Select(label => new GraphLabel(label)));
        public GraphToken Label(IEnumerable<GraphLabel> labels)
        {
            foreach (var label in labels.Where(label => !string.IsNullOrWhiteSpace(label.Value)))
                _labels.Add(label);
            return this;
        }

        public override bool Equals(object? obj)
        {
            return obj is GraphToken token && 
                _keys.SetEquals(token._keys);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            foreach (var key in _keys)
                hashCode.Add(key);
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            return $"Keys={string.Join(", ", _keys)}; Labels={string.Join(", ", _labels)}";
        }
    }
}
