namespace Repka.Strings
{
    public static class Extensions
    {
        public static bool ContainsIgnoreCase(this string source, string pattern)
            => source.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }
}
