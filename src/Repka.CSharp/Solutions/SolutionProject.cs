using Repka.Paths;

namespace Repka.Solutions
{
    public class SolutionProject
    {
        private static readonly Guid _csprojGuid = Guid.NewGuid();
        private static readonly Guid _folderGuid = Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        public static SolutionProject Csproj(AbsolutePath location, IEnumerable<SolutionPlatform> platforms) => 
            new(_csprojGuid, Path.GetFileNameWithoutExtension(location), location, platforms);

        public static SolutionProject Folder(AbsolutePath location) => 
            new(_folderGuid, Path.GetFileName(location), Path.GetFileName(location), Enumerable.Empty<SolutionPlatform>());

        public SolutionProject(Guid kind, string name, string description, IEnumerable<SolutionPlatform> platforms)
        {
            Id = Guid.NewGuid();
            Kind = kind;
            Name = name;
            Decription = description;
            Platforms = platforms;
        }

        public Guid Id { get; }

        public Guid Kind { get; }

        public string Name { get; set; }

        public string Decription { get; }

        public IEnumerable<SolutionPlatform> Platforms { get; }

        public string ToText() => $"Project(\"{Kind:B}\") = \"{Name}\", \"{Decription}\", \"{Id:B}\"\nEndProject";
    }
}
