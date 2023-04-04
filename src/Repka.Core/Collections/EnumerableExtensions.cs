namespace Repka.Collections
{
    public static class EnumerableExtensions
    {
        public static void ForAll<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static IEnumerable<TSource> Peek<TSource>(this IEnumerable<TSource> source, Action action)
        {
            return source.Peek(_ => action());
        }

        public static IEnumerable<TSource> Peek<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (var item in source)
            {
                action(item);
                yield return item;
            }
        }

        public static void AggregateTo<TSource>(this IEnumerable<TSource> source, ICollection<TSource> result)
        {
            foreach (var item in source)
            {
                result.Add(item);
            }
        }
    }
}
