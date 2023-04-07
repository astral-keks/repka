using System.Collections.Concurrent;

namespace Repka.Graphs
{
    public class GraphTraversal<TElement> : GraphTraversal<TElement, TElement>
        where TElement : notnull
    {
    }

    public class GraphTraversal<TSource, TResult>
        where TSource : notnull
    {
        private readonly ConcurrentDictionary<TSource, ICollection<TResult>?> _history = new();
        private readonly ConcurrentDictionary<TSource, TSource> _visiting = new();

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

            if (_visiting.TryAdd(element, element))
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
                    _visiting.TryRemove(element, out _);
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
