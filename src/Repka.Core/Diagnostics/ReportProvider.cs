namespace Repka.Diagnostics
{
    public class ReportProvider
    {
        public virtual ReportWriter GetWriter(string store, string name) => new();
    }
}
