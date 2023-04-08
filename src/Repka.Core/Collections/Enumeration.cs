using System.Collections.Concurrent;

namespace Repka.Collections
{
    public class Enumeration<TSource> : Enumeration<TSource, TSource>
        where TSource : notnull
    {
    }

    public class Enumeration<TSource, TResult>
        where TSource : notnull
    {
        private readonly ConcurrentDictionary<TSource, ICollection<TResult>?> _history = new();
        private readonly ConcurrentDictionary<TSource, TSource> _visiting = new();

        public ICollection<TResult> YieldAll(TSource element, Func<ICollection<TResult>> create) => Yield(element, create, Recall);

        public ICollection<TResult> YieldDistinct(TSource element, Func<ICollection<TResult>> create) => Yield(element, create, Bypass);

        private ICollection<TResult> Yield(TSource element, Func<ICollection<TResult>> create, Func<TSource, ICollection<TResult>?> recall)
        {
            ICollection<TResult>? results = default;

            if (_visiting.TryAdd(element, element))
            {
                try
                {
                    results = !_history.ContainsKey(element)
                        ? _history[element] = create()
                        : recall(element);
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
}
