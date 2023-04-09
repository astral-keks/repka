using System.Collections;

namespace Repka.Collections
{
    public static class Recursable
    {
        public static IRecursable<TSource> Recurse<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> select)
            where TSource : notnull
        {
            return new Recursable<TSource>(source.ToList(), select);
        }
    }

    public interface IRecursable<out TSource> : IEnumerable<TSource>
        where TSource : notnull
    {
        IEnumerable<TSource> Roots { get; }

        IEnumerable<TSource> Flatten(bool distinct = true);

        IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
        {
            return Roots.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


    public class Recursable<TSource> : IRecursable<TSource>
        where TSource : notnull
    {
        private readonly ICollection<TSource> _source;
        private readonly Func<TSource, IEnumerable<TSource>> _select;

        public Recursable(ICollection<TSource> source, Func<TSource, IEnumerable<TSource>> select)
        {
            _source = source;
            _select = select;
        }

        public IEnumerable<TSource> Roots 
        { 
            get => _source; 
        }

        public IEnumerable<TSource> Flatten(bool distinct = true)
        {
            Inspection<TSource> context = new();
            return Traverse(_source, distinct ? context.InspectOrIgnore : context.InspectOrGet);
        }

        public IEnumerable<TSource> Traverse(IEnumerable<TSource> source, Inspector<TSource> inspector)
        {
            return source.SelectMany(src => Traverse(src, inspector));
        }

        public ICollection<TSource> Traverse(TSource source, Inspector<TSource> inspector)
        {
            return inspector(source, () => Inspect().ToList());
            IEnumerable<TSource> Inspect()
            {
                yield return source;
                foreach (var item in Traverse(_select(source), inspector))
                    yield return item;
            }
        }
    }
}
