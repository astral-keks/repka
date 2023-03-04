namespace Repka.Gac
{
    public class GacProvider
    {
        public string? Root { get; init; }

        public GacDirectory GetGacDirectory()
        {
            return new GacDirectory(new[]
            {
                //RuntimeEnvironment.GetRuntimeDirectory(),
                Root ?? Environment.ExpandEnvironmentVariables(@"%windir%\Microsoft.NET\assembly\GAC_MSIL")
            });
        }
    }
}
