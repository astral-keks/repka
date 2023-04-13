namespace Repka.Projects
{
    internal class AssemblyReference
    {
        public static readonly AssemblyReference Mscorlib = new("mscorlib");
        public static readonly AssemblyReference Netstandard = new("netstandard");

        public AssemblyReference(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}
