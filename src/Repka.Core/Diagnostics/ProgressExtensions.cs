namespace Repka.Diagnostics
{
    public static class ProgressExtensions
    {
        public static ProgressPercentage Percent(this Progress progress, string title, int total)
        {
            return new ProgressPercentage(progress, title, total);
        }

        public static ProgressCounter Count(this Progress progress, string title, string? suffix = null)
        {
            return new ProgressCounter(progress, title, suffix ?? string.Empty);
        }
    }
}
