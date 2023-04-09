using System.Collections.Concurrent;

namespace Repka.Collections
{
    public class Inspection<TSource> : Inspection<TSource, TSource>
        where TSource : notnull
    {
    }

    public class Inspection<TSource, TResult>
        where TSource : notnull
    {
        private readonly ConcurrentDictionary<TSource, ICollection<TResult>?> _history = new();
        private readonly ConcurrentDictionary<TSource, TSource> _visiting = new();

        public ICollection<TResult> InspectOrIgnore(TSource element, Func<ICollection<TResult>> create) => Inspect(element, create, Ignore);

        public ICollection<TResult> InspectOrGet(TSource element, Func<ICollection<TResult>> create) => Inspect(element, create, Get);

        private ICollection<TResult> Inspect(TSource element, Func<ICollection<TResult>> create, Func<TSource, ICollection<TResult>?> recall)
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

        private ICollection<TResult>? Get(TSource element) => _history[element];

        private ICollection<TResult>? Ignore(TSource _) => default;
    }

    public delegate ICollection<TSource> Inspector<TSource>(TSource element, Func<ICollection<TSource>> create);

    public delegate ICollection<TResult> Inspector<TSource, TResult>(TSource element, Func<ICollection<TResult>> create);
}
