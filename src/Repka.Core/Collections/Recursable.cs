using System.Collections;

namespace Repka.Collections
{
    public static class Recursable
    {
        public static Recursable<TSource> Recurse<TSource>(this TSource source, Func<TSource, IEnumerable<TSource>> select,
            Enumeration<TSource>? context = default)
            where TSource : notnull
        {
            return source.AsEnumerable().Recurse(select, context ?? new Enumeration<TSource>());
        }

        public static Recursable<TSource> Recurse<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> select,
            Enumeration<TSource>? context = default)
            where TSource : notnull
        {
            context ??= new Enumeration<TSource>();
            return new Recursable<TSource>(source, select, context);
        }
    }

    public class Recursable<TSource> : IEnumerable<TSource>
        where TSource : notnull
    {
        private readonly IEnumerable<TSource> _source;
        private readonly Func<TSource, IEnumerable<TSource>> _select;
        private readonly Enumeration<TSource> _context;

        public Recursable(IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> select, Enumeration<TSource> context)
        {
            _source = source;
            _select = select;
            _context = context;
        }

        public IEnumerable<TSource> All()
        {
            return _source.SelectMany(source => _context.YieldAll(source, () => ToCollection(source)));
        }

        public IEnumerable<TSource> Distinct()
        {
            return _source.SelectMany(source => _context.YieldDistinct(source, () => ToCollection(source)));
        }

        private ICollection<TSource> ToCollection(TSource source)
        {
            return _select(source).Prepend(source)
                .Recurse(_select, _context)
                .ToList();
        }

        public IEnumerator<TSource> GetEnumerator()
        {
            return All().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
