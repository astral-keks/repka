using System.Diagnostics;

namespace Repka.Diagnostics
{
    public sealed class Benchmark : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _watch = new();
        private readonly BenchmarkCollection _elements = new();

        public Benchmark(string name)
        {
            _name = name;
        }

        public string Name => _name;

        public TimeSpan Duration => _watch.Elapsed;

        public Benchmark this[string name] => _elements[name];

        public Report Report => new()
        {
            Text = $"{_name}: {Duration}",
            Records = _elements
                .Select(element =>
                {
                    Report record = element.Report;
                    record.Text = $"{record.Text} ({(int)(100 * element.Duration.TotalMilliseconds / Duration.TotalMilliseconds)}%)";
                    return record;
                })
                .ToList()
        };

        public Benchmark Start()
        {
            if (!_watch.IsRunning)
                _watch.Start();
            foreach (var element in _elements)
                element.Reset();
            return this;
        }

        public Benchmark Pause()
        {
            if (_watch.IsRunning)
                _watch.Stop();
            foreach (var element in _elements)
                element.Reset();
            return this;
        }

        public Benchmark Stop()
        {
            Pause();
            _watch.Reset();
            foreach (var element in _elements)
                element.Stop();
            return this;
        }

        public Benchmark Reset()
        {
            _watch.Reset();
            foreach (var element in _elements)
                element.Reset();
            return this;
        }

        public Benchmark Print(ReportWriter? writer = default)
        {
            writer ??= ReportWriterTextual.Console;
            writer.Write(Report);
            return this;
        }

        public void Dispose()
        {
            Pause();
        }
    }
}
