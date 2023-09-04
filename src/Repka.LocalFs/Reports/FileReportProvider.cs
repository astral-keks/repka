namespace Repka.Diagnostics
{
    public class FileReportProvider : ReportProvider
    {
        private readonly string _root;

        public FileReportProvider(string root)
        {
            _root = root;
        }

        public DirectoryReportProvider AsDirectory() => new(_root);

        public override ReportWriter GetWriter(string name)
        {
            string location = Path.Combine(_root, $"{name}-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.txt");
            string? directory = Path.GetDirectoryName(location);
            if (directory is not null )
                Directory.CreateDirectory(directory);

            StreamWriter writer = new(location);
            return new FileReportWriter(writer);
        }
    }
}
