using System.Collections;
using System.Collections.Concurrent;

namespace Repka.Diagnostics
{
    internal class BenchmarkCollection : IEnumerable<Benchmark>
    {
        private readonly ConcurrentDictionary<string, Benchmark> _elements = new();

        public Benchmark this[string name] => _elements.GetOrAdd(name, _ => new Benchmark(name));

        public IEnumerator<Benchmark> GetEnumerator() => _elements.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
