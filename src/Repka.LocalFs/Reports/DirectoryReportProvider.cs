namespace Repka.Diagnostics
{
    public class DirectoryReportProvider : ReportProvider
    {
        private readonly string _root;

        public DirectoryReportProvider(string root)
        {
            _root = root;
        }

        public override ReportWriter GetWriter(string name)
        {
            string directory = Path.Combine(_root, $"{name}-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}");
            Directory.CreateDirectory(directory);

            return new DirectoryReportWriter(directory);
        }
    }
}
