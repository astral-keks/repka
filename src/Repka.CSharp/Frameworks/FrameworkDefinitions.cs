using Repka.Assemblies;
using System.Runtime.InteropServices;

namespace Repka.Frameworks
{
    public static class FrameworkDefinitions
    {
        public static readonly FrameworkDefinition Current = new("net6.0",
            new AssemblyResolver(new List<string>()
            {
                RuntimeEnvironment.GetRuntimeDirectory()
            }));

        public static readonly FrameworkDefinition Net48 = new("net48",
            new(new List<string>() 
            {
                @"C:\WINDOWS\Microsoft.Net\Framework64\v4.0.30319",
                @"C:\WINDOWS\Microsoft.Net\Framework64\v4.0.30319\WPF",
            }),
            new List<string>()
            {
                "mscorlib",
                "System",
                "System.Core",
                "System.Data",
                "System.Drawing",
                "System.IO.Compression",
                "System.IO.Compression.FileSystem",
                "System.Numerics",
                "System.Runtime",
                "System.ServiceModel",
                "System.Runtime.Serialization",
                "System.Web",
                "System.Xml",
                "System.Xml.Linq",
            });


        public static readonly FrameworkDefinition VisualStudio2022 = new(null,
            new(new List<string>()
            {
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\PublicAssemblies",
            }));
    }
}
