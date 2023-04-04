namespace Repka.Collections
{
    public static class CollectionExtensions
    {
        public static void AddRange<TSource>(this ICollection<TSource> collection, IEnumerable<TSource> source)
        {
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }

    }
}
