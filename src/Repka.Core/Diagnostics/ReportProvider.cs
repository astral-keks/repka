namespace Repka.Diagnostics
{
    public class ReportProvider
    {
        public virtual ReportWriter GetWriter(string name) => new();
    }
}
