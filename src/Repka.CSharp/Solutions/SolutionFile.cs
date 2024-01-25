using Repka.Paths;
using System.Text.RegularExpressions;

namespace Repka.Solutions
{
    public class SolutionFile
    {
        private static readonly Regex _csprojPattern = new(@"([^""]+\.csproj)""");

        private readonly SolutionProjectCollection _projects = new();

        public static SolutionFile Parse(AbsolutePath path)
        {
            List<AbsolutePath> projects = File.ReadAllLines(path)
                .Select(line => _csprojPattern.Match(line))
                .Where(match => match.Groups[1].Success)
                .Select(match => match.Groups[1].Value)
                .Select(relativePath => path.Parent()?.Combine(relativePath))
                .OfType<AbsolutePath>()
                .ToList();
            return new SolutionFile(projects);
        }

        public SolutionFile(IEnumerable<AbsolutePath> projectLocations)
        {
            _projects.AddRange(projectLocations, Platforms);
        }

        public Guid RootGuid { get; } = Guid.NewGuid();

        public IEnumerable<SolutionProject> Projects => _projects;

        public IEnumerable<SolutionPlatform> Platforms => new[] { SolutionPlatform.DebugAnyCPU, SolutionPlatform.ReleaseAnyCPU };

        public IEnumerable<SolutionSection> Sections => new[]
        {
            SolutionSection.SolutionConfigurationPlatforms(Platforms),
            SolutionSection.ProjectConfigurationPlatforms(Projects),
            SolutionSection.SolutionProperties(),
            SolutionSection.NestedProjects(_projects.AsRelations()),
            SolutionSection.ExtensibilityGlobals(),
        };

        public string ToText()
        {
            return $"Microsoft Visual Studio Solution File, Format Version 12.00\n" +
                $"# Visual Studio Version 17\n" +
                $"VisualStudioVersion = 17.4.33110.190\n" +
                $"MinimumVisualStudioVersion = 10.0.40219.1\n" +
                $"{string.Join("\n", Projects.Select(project => project.ToText()))}\n" +
                $"Global\n" +
                $"{string.Join("\n", Sections.Select(section => section.ToText("    ")))}\n" +
                $"EndGlobal\n";
        }
    }
}
