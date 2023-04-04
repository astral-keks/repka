namespace Repka.Diagnostics
{
    public class ProgressCounter
    {
        private readonly Progress _progress;
        private readonly string _title;
        private readonly string _suffix;
        private int _count;

        public ProgressCounter(Progress progress, string title, string suffix)
        {
            _progress = progress;
            _title = title;
            _suffix = suffix;

        }

        public void Increment()
        {
            _progress.Notify($"{_title}: {_count++} {_suffix}");
        }

        public void Complete()
        {
            _progress.Finish($"{_title}: {_count} {_suffix}");
        }

        public void Reset()
        {
            _count = 0;
            _progress.Start($"{_title}");
        }
    }
}
