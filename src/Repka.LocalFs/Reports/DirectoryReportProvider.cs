using Repka.FileSystems;

namespace Repka.Diagnostics
{
    public class DirectoryReportProvider : ReportProvider
    {
        public override ReportWriter GetWriter(string store, string name)
        {
            string directory = Path.Combine(FileSystemPaths.Aux(store), $"{name}-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}");
            Directory.CreateDirectory(directory);

            return new DirectoryReportWriter(directory);
        }
    }
}
