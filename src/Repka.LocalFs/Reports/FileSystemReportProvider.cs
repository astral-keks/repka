using Repka.FileSystems;

namespace Repka.Diagnostics
{
    public class FileSystemReportProvider : ReportProvider
    {
        public string? Root { get; init; }

        public override ReportWriter GetWriter(string store, string name)
        {
            string directory = FileSystemPaths.GetRootPath(Root, store);
            Directory.CreateDirectory(directory);

            string location = Path.Combine(directory, $"{name}.txt");
            StreamWriter writer = new(location);
            return new FileSystemReportWriter(writer);
        }
    }
}
