namespace Repka.Diagnostics
{
    internal class FileSystemReportWriter : ReportWriter
    {
        private readonly TextWriter _writer;

        public FileSystemReportWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Write(Report report)
        {
            lock(_writer)
                Write(report, 0);
            _writer.Flush();
        }

        private void Write(Report report, int indent)
        {
            _writer.WriteLine($"{string.Concat(Enumerable.Repeat("\t", indent))}{report.Text}");
            foreach (var record in report.Records)
            {
                Write(record, indent + 1);
            }
        }

        public override void Dispose()
        {
            _writer.Dispose();
        }
    }
}
