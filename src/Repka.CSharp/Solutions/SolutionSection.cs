namespace Repka.Solutions
{
    public class SolutionSection
    {
        public static SolutionSection SolutionConfigurationPlatforms(IEnumerable<SolutionPlatform> platforms) =>
            new("SolutionConfigurationPlatforms", "preSolution",
                platforms.Select(platform => new SolutionSectionItem(platform.ToString(), platform.ToString()))
                .ToArray());

        public static SolutionSection ProjectConfigurationPlatforms(IEnumerable<SolutionProject> projects) =>
            new("ProjectConfigurationPlatforms", "postSolution", projects
                .SelectMany(project => project.Platforms.SelectMany(platform => new[]
                {
                    new SolutionSectionItem($"{project.Id:B}.{platform}.ActiveCfg", platform.ToString()),
                    new SolutionSectionItem($"{project.Id:B}.{platform}.Build.0", platform.ToString()),
                }))
                .ToArray());

        public static SolutionSection NestedProjects(IEnumerable<(SolutionProject Project, SolutionProject Folder)> relations) => 
            new("NestedProjects", "preSolution", relations
                .Select(relation => new SolutionSectionItem(relation.Project.Id.ToString("B"), relation.Folder.Id.ToString("B")))
                .ToArray());
        
        public static SolutionSection SolutionProperties() => 
            new("SolutionProperties", "preSolution", 
                new SolutionSectionItem("HideSolutionNode", "FALSE"));

        public static SolutionSection ExtensibilityGlobals() => 
            new("ExtensibilityGlobals", "postSolution", 
                new SolutionSectionItem("SolutionGuid", Guid.NewGuid().ToString("B")));
        
        public SolutionSection(string name, string position, params SolutionSectionItem[] items)
        {
            Name = name;
            Hook = position;
            Items = items;
        }
        public string Name { get; }

        public string Hook { get; }

        public IEnumerable<SolutionSectionItem> Items { get; }

        public string ToText(string indent)
        {
            return $"{indent}GlobalSection({Name}) = {Hook}\n" +
                $"{indent}    {string.Join($"\n{indent}    ", Items)}\n" +
                $"{indent}EndGlobalSection";
        }
    }
}
