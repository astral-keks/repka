namespace Repka.Diagnostics
{
    public class Progress
    {
        public virtual void Start(string message)
        {
        }

        public virtual void Notify(string progress)
        {
        }

        public virtual void Finish(string message)
        {
        }

        public class StdIO : Progress
        {
            public override void Start(string message)
            {
                Console.WriteLine(message);
            }

            public override void Notify(string progress)
            {
                Console.WriteLine(progress);
            }

            public override void Finish(string message)
            {
                Console.WriteLine(message);
            }
        }
    }

    public static class GraphProgressExtensions
    {
        public static IEnumerable<TSource> WithProgress<TSource>(this ICollection<TSource> source, Progress progress, string title)
        {
            int count = 0;
            progress.Start($"{title}:");
            foreach (var item in source)
            {
                progress.Notify($"{title}: {count++} of {source.Count}");
                yield return item;
            }
            progress.Finish($"{title}: {count}");
        }
    }
}
