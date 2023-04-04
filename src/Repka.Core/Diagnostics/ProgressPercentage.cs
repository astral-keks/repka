namespace Repka.Diagnostics
{
    public class ProgressPercentage
    {
        private readonly Progress _progress;
        private readonly string _title;
        private readonly int _total;
        private int _value;

        public ProgressPercentage(Progress progress, string title, int total)
        {
            _progress = progress;
            _title = title;
            _total = total;
        }

        public int Value => (int)Math.Round((double)_value / _total * 100);

        public void Increment() => Add(1);

        public void Add(int value)
        {
            int current = Value;
            _value += value;
            if (_value > _total)
                _value %= _total;
            if (Value != current)
                _progress.Notify($"{_title}: {Value}%");
        }

        public void Complete()
        {
            _progress.Finish($"{_title}: {Value}%");
        }

        public void Reset()
        {
            _value = 0;
            _progress.Start($"{_title}");
        }
    }
}
