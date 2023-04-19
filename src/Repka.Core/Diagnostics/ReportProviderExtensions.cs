namespace Repka.Diagnostics
{
    public static class ReportProviderExtensions
    {
        public static void Write(this ReportProvider provider, Report report)
        {
            provider.Write(report.Title, report);
        }


        public static void Write(this ReportProvider provider, string name,  Report report)
        {
            using ReportWriter writer = provider.GetWriter(name);
            writer.Write(report);
        }
    }
}
