using System.Runtime.InteropServices;

namespace Repka.Frameworks
{
    public class FrameworkProvider
    {
        public string? Root { get; init; }

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
            return new FrameworkDirectory(Root ?? RuntimeEnvironment.GetRuntimeDirectory(), Assemblies);
        }
    }
}
