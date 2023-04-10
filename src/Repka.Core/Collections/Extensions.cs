namespace Repka.Collections
{
    public static class Extensions
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
            return source.Select(item =>
            {
                action(item);
                return item;
            });
        }

        public static void AddRange<TSource>(this ICollection<TSource> collection, IEnumerable<TSource> source)
        {
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }

        public static ParallelQuery<TSource> Peek<TSource>(this ParallelQuery<TSource> source, Action action)
        {
            return source.Peek(_ => action());
        }

        public static ParallelQuery<TSource> Peek<TSource>(this ParallelQuery<TSource> source, Action<TSource> action)
        {
            return source.Select(item =>
            {
                action(item);
                return item;
            });
        }

        public static ParallelQuery<TSource> AsParallel<TSource>(this IEnumerable<TSource> source, int degreeOfParallelism)
        {
            return source.AsParallel().WithDegreeOfParallelism(degreeOfParallelism).WithMergeOptions(ParallelMergeOptions.NotBuffered);
        }
    }
}
