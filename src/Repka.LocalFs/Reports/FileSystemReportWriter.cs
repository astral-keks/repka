namespace Repka.Reports
{
    internal class FileSystemReportWriter : ReportWriter
    {
        private readonly StreamWriter _writer;

        public FileSystemReportWriter(string location)
        {
            _writer = new(location);
        }

        public override void Write(Report report)
        {
            lock(_writer)
            {
                _writer.WriteLine(report.Title);
                foreach (var record in report.Records)
                {
                    _writer.WriteLine($"\t{record}");
                }
            }
        }

        public override void Dispose()
        {
            _writer.Dispose();
        }
    }
}
