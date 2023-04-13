using Repka.Graphs;
using Repka.Strings;

namespace Repka.Assemblies
{
    public class AssemblyName : Normalized
    {
        public static implicit operator AssemblyName(string value) => new(value);
        public AssemblyName(string value) : base(value) { }
    }

    public static class AssemblyNameExtensions
    {
        public static AssemblyName AsAssemblyName(this GraphKey source) => new(source);
    }
}
