namespace Repka.Graphs
{
    public readonly struct GraphTag
    {
        public GraphTag(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public string Value { get; }

        public override string ToString() => $"{Name}:{Value}";
    }
}
