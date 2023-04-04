namespace Repka.Projects
{
    internal class FrameworkReference
    {
        public static readonly FrameworkReference Mscorlib = new("mscorlib");
        public static readonly FrameworkReference Netstandard = new("netstandard");

        public FrameworkReference(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
