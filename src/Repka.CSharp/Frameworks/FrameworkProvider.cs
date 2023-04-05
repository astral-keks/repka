using System.Runtime.InteropServices;

namespace Repka.Frameworks
{
    public class FrameworkProvider
    {
        public List<string> Roots { get; init; } = new();

        public List<string> Assemblies { get; init; } = new()
        {
            "System",
            "System.Core",
            "System.Data",
            "System.Drawing",
            "System.IO.Compression.FileSystem",
            "System.Numerics",
            "System.ServiceModel",
            "System.Runtime.Serialization",
            "System.Web",
            "System.Xml",
            "System.Xml.Linq",
        };

        public FrameworkDirectory GetFrameworkDirectory()
        {
            List<string> roots = Roots.Any() ? Roots : new() { RuntimeEnvironment.GetRuntimeDirectory() };
            return new FrameworkDirectory(roots, Assemblies);
        }
    }
}
