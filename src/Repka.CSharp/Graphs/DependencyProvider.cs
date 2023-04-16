using Repka.Collections;
using Repka.Diagnostics;
using Repka.Frameworks;
using static Repka.Graphs.DependencyDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class DependencyProvider : GraphProvider
    {
        public FrameworkDefinition TargetFramework { get; init; } = FrameworkDefinitions.Current;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            List<ProjectNode> projectNodes = graph.Projects().ToList();
            List<PackageNode> packageNodes = graph.Packages().ToList();

            ProgressPercentage projectProgress = Progress.Percent("Restoring projects", projectNodes.Count);
            Inspection<ProjectNode, (ProjectNode, DependencyKind, DependencyOrigin)> projectInspection = new();
            IEnumerable<GraphToken> projectTokens = projectNodes
                .Peek(projectProgress.Increment)
                .SelectMany(projectNode => GetProjectTokens(projectNode, projectInspection))
                .ToList();
            foreach (var token in projectTokens)
                graph.Add(token);
            projectProgress.Complete();

            Inspection<PackageNode, (PackageNode, DependencyKind, DependencyOrigin)> packageInspection = new();
            ProgressPercentage packageProgress = Progress.Percent("Restoring packages", projectNodes.Count);
            IEnumerable<GraphToken> packageTokens = packageNodes
                .Peek(packageProgress.Increment)
                .SelectMany(packageNode => GetPackageTokens(packageNode, packageInspection))
                .ToList();
            foreach (var token in packageTokens)
                graph.Add(token);
            packageProgress.Complete();

        }

        private IEnumerable<GraphToken> GetProjectTokens(ProjectNode projectNode, 
            Inspection<ProjectNode, (ProjectNode, DependencyKind, DependencyOrigin)> projectInspection)
        {
            foreach (var (dependencyProject, dependencyKind, dependencyOrigin) in GetProjectDependencies(projectNode, projectInspection))
            {
                if (projectNode.HasSdk || dependencyKind == DependencyKind.Direct || dependencyOrigin == DependencyOrigin.Package)
                {
                    GraphLinkToken token = new(projectNode.Key, dependencyProject.Key, DependencyLabels.Project);
                    token.Mark(DependencyLabels.Kind, dependencyKind.ToString());
                    yield return token;
                }
            }
        }

        private IEnumerable<(ProjectNode Project, DependencyKind Kind, DependencyOrigin Origin)> GetProjectDependencies(ProjectNode projectNode, 
            Inspection<ProjectNode, (ProjectNode, DependencyKind, DependencyOrigin)> projectInspection)
        {
            return projectInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<(ProjectNode, DependencyKind, DependencyOrigin)> visitProject()
            {
                foreach (var referencedProject in projectNode.ReferencedProjects().ToList())
                {
                    yield return new(referencedProject, DependencyKind.Direct, DependencyOrigin.Project);
                    foreach (var dependency in GetProjectDependencies(referencedProject, projectInspection))
                        yield return dependency with { Kind = DependencyKind.Transitive};
                }

                foreach (var referencedPackage in projectNode.ReferencedPackages().ToList())
                {
                    ProjectNode? packagableProject = referencedPackage.Project();
                    if (packagableProject is not null)
                    {
                        yield return new(packagableProject, DependencyKind.Direct, DependencyOrigin.Package);
                        foreach (var dependency in GetProjectDependencies(packagableProject, projectInspection))
                            yield return dependency with { Kind = DependencyKind.Transitive, Origin = DependencyOrigin.Package };
                    }
                }
            }
        }


        private IEnumerable<GraphToken> GetPackageTokens(PackageNode packageNode,
            Inspection<PackageNode, (PackageNode, DependencyKind, DependencyOrigin)> packageInspection)
        {
            foreach (var (dependencyProject, dependencyKind, _) in GetPackageDependencies(packageNode, packageInspection))
            {
                GraphLinkToken token = new(packageNode.Key, dependencyProject.Key, DependencyLabels.Package);
                token.Mark(DependencyLabels.Kind, dependencyKind.ToString());
                yield return token;
            }
        }

        private IEnumerable<(PackageNode Package, DependencyKind Kind, DependencyOrigin Origin)> GetPackageDependencies(PackageNode packageNode,
            Inspection<PackageNode, (PackageNode, DependencyKind, DependencyOrigin)> packageInspection)
        {
            return packageInspection.InspectOrGet(packageNode, () => visitPackage().ToHashSet());
            IEnumerable<(PackageNode, DependencyKind, DependencyOrigin)> visitPackage()
            {
                foreach (var referencedPackage in packageNode.ReferencedPackages(TargetFramework).ToList())
                {
                    yield return new(referencedPackage, DependencyKind.Direct, DependencyOrigin.Package);

                    foreach (var dependency in GetPackageDependencies(referencedPackage, packageInspection))
                        yield return dependency with { Kind = DependencyKind.Transitive };
                }
            }
        }
    }
}
