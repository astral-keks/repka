namespace Repka.Diagnostics
{
    internal class ReportWriterTextual : ReportWriter
    {
        private readonly TextWriter _writer;

        public static ReportWriterTextual Console { get; } = new(System.Console.Out);

        public static ReportWriterTextual File(string path) => new(new StreamWriter(path));

        public ReportWriterTextual(TextWriter writer)
        {
            _writer = writer;
        }

        public override void Write(Report report)
        {
            lock (_writer)
                Write(report, 0);
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
