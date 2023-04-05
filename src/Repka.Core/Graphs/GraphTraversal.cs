namespace Repka.Graphs
{
    public class GraphTraversal<TElement> : GraphTraversal<TElement, TElement>
        where TElement : notnull
    {
    }

    public class GraphTraversal<TSource, TResult>
        where TSource : notnull
    {
        private readonly Dictionary<TSource, ICollection<TResult>?> _history = new();
        private readonly HashSet<TSource> _visiting = new();

        public GraphTraversalStrategy Strategy { get; init; } = GraphTraversalStrategy.RecallHistory;

        public TResult? Visit(TSource element, Func<TResult> factory) => 
            Visit(element, () => new[] { factory() }).SingleOrDefault();

        public ICollection<TResult> Visit(TSource element, Action<ICollection<TResult>> factory) =>
            Visit(element, () =>
            {
                HashSet<TResult> results = new();
                factory(results);
                return results;
            });

        public ICollection<TResult> Visit(TSource element, Func<ICollection<TResult>> factory)
        {
            ICollection<TResult>? results = default;

            if (_visiting.Add(element))
            {
                try
                {
                    results = !_history.ContainsKey(element)
                        ? _history[element] = factory()
                        : Strategy switch
                        {
                            GraphTraversalStrategy.BypassHistory => Bypass(element),
                            GraphTraversalStrategy.RecallHistory or _ => Recall(element),
                        };
                }
                finally
                {
                    _visiting.Remove(element);
                }
            }

            return results ?? new List<TResult>(0);
        }

        private ICollection<TResult>? Recall(TSource element) => _history[element];

        private ICollection<TResult>? Bypass(TSource _) => default;
    }

    public enum GraphTraversalStrategy
    {
        RecallHistory,
        BypassHistory
    }
}
