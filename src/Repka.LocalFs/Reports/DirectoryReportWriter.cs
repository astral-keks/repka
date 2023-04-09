namespace Repka.Diagnostics
{
    internal class DirectoryReportWriter : ReportWriter
    {
        private readonly string _directory;

        public DirectoryReportWriter(string directory)
        {
            _directory = directory;
        }

        public override void Write(Report report)
        {
            string location = Path.Combine(_directory, $"{report.Title}.txt");
            File.WriteAllLines(location, ToLines(report, 0));
        }

        private IEnumerable<string> ToLines(Report report, int indent)
        {
            yield return $"{string.Concat(Enumerable.Repeat("\t", indent))}{report.Text}";
            foreach (var record in report.Records)
            {
                foreach (var line in ToLines(record, indent + 1))
                    yield return line;
            }
        }
    }
}
