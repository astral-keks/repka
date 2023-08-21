using static Repka.Graphs.AssemblyDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class RestoreDsl
    {
        public static IEnumerable<ProjectNode> RestoredProjects(this ProjectNode project) => 
            project.Outputs(RestoreLabels.ProjectDependency)
                .Select(link => new ProjectDependencyLink(link))
                .Select(dependency => dependency.TargetProject());

        public static IEnumerable<ProjectNode> RestoringProjects(this ProjectNode project) => 
            project.Inputs(RestoreLabels.ProjectDependency)
                .Select(link => new ProjectDependencyLink(link))
                .Select(dependency => dependency.SourceProject());


        public static IEnumerable<PackageNode> RestoredPackages(this ProjectNode project) => RestoredPackages((GraphNode)project);
        public static IEnumerable<PackageNode> RestoredPackages(this PackageNode package) => RestoredPackages((GraphNode)package);
        private static IEnumerable<PackageNode> RestoredPackages(this GraphNode node) => 
            node.Outputs(RestoreLabels.PackageDependency)
                .Select(link => new PackageDependencyLink(link))
                .Select(dependency => dependency.TargetPackage());

        public static IEnumerable<PackageNode> RestoringPackages(this PackageNode package) => 
            package.Inputs(RestoreLabels.PackageDependency)
                .Select(link => new PackageDependencyLink(link))
                .Select(dependency => dependency.SourcePackage());


        public static IEnumerable<AssemblyNode> RestoredAssemblies(this ProjectNode project) => RestoredAssemblies((GraphNode)project);
        public static IEnumerable<AssemblyNode> RestoredAssemblies(this PackageNode package) => RestoredAssemblies((GraphNode)package);
        private static IEnumerable<AssemblyNode> RestoredAssemblies(this GraphNode package) => 
            package.Outputs(RestoreLabels.AssemblyDependency)
                .Select(link => new AssemblyDependencyLink(link))
                .Select(dependency => dependency.TargetAssembly());


        public class ProjectDependencyLink : DependencyLink
        {
            public ProjectDependencyLink(GraphLink link) : base(link) { }

            public ProjectNode SourceProject() => Source().AsProject()
                ?? throw new InvalidOperationException("Source is not project");

            public ProjectNode TargetProject() => Target().AsProject()
                ?? throw new InvalidOperationException("Target is not project");
        }

        public class PackageDependencyLink : DependencyLink
        {
            public PackageDependencyLink(GraphLink link) : base(link) { }

            public PackageNode SourcePackage() => Source().AsPackage()
                ?? throw new InvalidOperationException("Source is not package");

            public PackageNode TargetPackage() => Target().AsPackage()
                ?? throw new InvalidOperationException("Target is not package");
        }

        public class AssemblyDependencyLink : DependencyLink
        {
            public AssemblyDependencyLink(GraphLink link) : base(link) { }

            public AssemblyNode SourceAssembly() => Source().AsAssembly()
                ?? throw new InvalidOperationException("Source is not assembly");

            public AssemblyNode TargetAssembly() => Target().AsAssembly()
                ?? throw new InvalidOperationException("Target is not assembly");
        }

        public abstract class DependencyLink : GraphLink
        {
            public DependencyLink(GraphLink link) : base(link) { }

            public DependencyKind Kind => Tag(RestoreLabels.Kind)
                .Map(tag => Enum.TryParse(tag.Value, out DependencyKind kind) ? kind : DependencyKind.Unknown)
                .OrElse(DependencyKind.Unknown);

            public DependencyOrigin Origin => Tag(RestoreLabels.Origin)
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

        public static class RestoreLabels
        {
            public const string Dependency = nameof(Dependency);
            public const string ProjectDependency = $"{Dependency}.{nameof(ProjectDependency)}";
            public const string PackageDependency = $"{Dependency}.{nameof(PackageDependency)}";
            public const string AssemblyDependency = $"{Dependency}.{nameof(AssemblyDependency)}";

            public const string Kind = $"{Dependency}.{nameof(Kind)}";
            public const string Origin = $"{Dependency}.{nameof(Origin)}";
        }
    }
}
