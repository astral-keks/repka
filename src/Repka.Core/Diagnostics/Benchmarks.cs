namespace Repka.Diagnostics
{
    public static class Benchmarks
    {
        private static Benchmark Root { get; } = new("Root");

        public static Benchmark Start(string? name = default) => Get(name).Start();

        public static Benchmark Stop(string? name = default) => Get(name).Pause();

        private static Benchmark Get(string? name = default) => name is not null ? Root[name] : Root;
    }
}
