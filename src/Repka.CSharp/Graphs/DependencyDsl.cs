using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class DependencyDsl
    {
        public static IEnumerable<ProjectDependencyLink> ProjectDependencies(this ProjectNode project) => project.Outputs(DependencyLabels.Project)
            .Select(link => new ProjectDependencyLink(link));

        public static IEnumerable<ProjectNode> DependencyProjects(this ProjectNode project) => project.Outputs(DependencyLabels.Project)
            .Select(link => new ProjectDependencyLink(link))
            .Select(dependency => dependency.TargetProject());

        public static IEnumerable<ProjectNode> DependentProjects(this ProjectNode project) => project.Inputs(DependencyLabels.Project)
            .Select(link => new ProjectDependencyLink(link))
            .Select(dependency => dependency.SourceProject());


        public class ProjectDependencyLink : GraphLink
        {
            public ProjectDependencyLink(GraphLink link) : base(link) { }

            public ProjectNode SourceProject() => Source().AsProject()
                ?? throw new InvalidOperationException("Source is not project");

            public ProjectNode TargetProject() => Target().AsProject()
                ?? throw new InvalidOperationException("Target is not project");

            public DependencyKind Kind => Tag(DependencyLabels.Kind)
                .Map(tag => Enum.TryParse(tag.Value, out DependencyKind kind) ? kind : DependencyKind.Unknown)
                .OrElse(DependencyKind.Unknown);

            public DependencyOrigin Origin => Tag(DependencyLabels.Origin)
                .Map(tag => Enum.TryParse(tag.Value, out DependencyOrigin kind) ? kind : DependencyOrigin.Unknown)
                .OrElse(DependencyOrigin.Unknown);
        }

        public enum DependencyOrigin
        {
            Unknown,
            Project,
            Package,
        }

        public enum DependencyKind
        {
            Unknown,
            Direct,
            Transitive,
        }

        public static class DependencyLabels
        {
            public const string Dependency = nameof(Dependency);
            public const string Project = $"{Dependency}.{nameof(Project)}";
            public const string Package = $"{Dependency}.{nameof(Package)}";

            public const string Kind = $"{Dependency}.{nameof(Kind)}";
            public const string Origin = $"{Dependency}.{nameof(Origin)}";
        }
    }
}
