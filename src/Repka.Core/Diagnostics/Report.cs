namespace Repka.Diagnostics
{
    public class Report
    {
        public string Title { get; set; } = Guid.NewGuid().ToString();

        public string Text { get; set; } = string.Empty;

        public IEnumerable<Report> Records { get; set; } = Enumerable.Empty<Report>();
    }
}
