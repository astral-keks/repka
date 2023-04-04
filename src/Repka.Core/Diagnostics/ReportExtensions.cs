namespace Repka.Diagnostics
{
    public static class ReportExtensions
    {
        public static void Save(this Report report, string path)
        {
            using ReportWriter writer = ReportWriterTextual.File(path);
            writer.Write(report);
        }
    }
}
