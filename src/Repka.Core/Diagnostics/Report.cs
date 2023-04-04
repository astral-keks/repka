namespace Repka.Diagnostics
{
    public class Report
    {
        public string Text { get; set; } = "";

        public IEnumerable<Report> Records { get; set; } = Enumerable.Empty<Report>();
    }
}
