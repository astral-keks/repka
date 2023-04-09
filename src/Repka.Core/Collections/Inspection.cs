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

        public ICollection<TResult> InspectOrGet(TSource element, Func<ICollection<TResult>> create) => Inspect(element, create, Recall);

        private ICollection<TResult> Inspect(TSource element, Func<ICollection<TResult>> create, 
            Func<ICollection<TResult>?, ICollection<TResult>?> reinspect)
        {
            ICollection<TResult>? results = default;

            if (_visiting.TryAdd(element, element))
            {
                try
                {
                    bool added = false;
                    results = _history.GetOrAdd(element, _ =>
                    {
                        added = true;
                        return create();
                    });

                    if (!added)
                        results = reinspect(results);
                }
                finally
                {
                    _visiting.TryRemove(element, out _);
                }
            }

            return results ?? new List<TResult>(0);
        }

        private ICollection<TResult>? Recall(ICollection<TResult>? results) => results;

        private ICollection<TResult>? Ignore(ICollection<TResult>? _) => default;
    }

    public delegate ICollection<TSource> Inspector<TSource>(TSource element, Func<ICollection<TSource>> create);

    public delegate ICollection<TResult> Inspector<TSource, TResult>(TSource element, Func<ICollection<TResult>> create);
}
