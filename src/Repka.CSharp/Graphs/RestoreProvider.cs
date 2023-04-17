using Repka.Collections;
using Repka.Diagnostics;
using Repka.Frameworks;
using static Repka.Graphs.RestoreDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;
using static Repka.Graphs.AssemblyDsl;

namespace Repka.Graphs
{
    public class RestoreProvider : GraphProvider
    {
        public FrameworkDefinition TargetFramework { get; init; } = FrameworkDefinitions.Current;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Restoring projects", projectNodes.Count);
            IEnumerable<GraphToken> projectTokens = GetProjectTokens(projectNodes.Peek(projectProgress.Increment));
            foreach (var token in projectTokens)
                graph.Add(token);
            projectProgress.Complete();

            List<PackageNode> packageNodes = graph.Packages().ToList();
            ProgressPercentage packageProgress = Progress.Percent("Restoring packages", packageNodes.Count + projectNodes.Count);
            IEnumerable<GraphToken> packageTokens = GetPackageTokens(packageNodes.Peek(packageProgress.Increment), 
                projectNodes.Peek(packageProgress.Increment));
            foreach (var token in packageTokens)
                graph.Add(token);
            packageProgress.Complete();

        }

        private IEnumerable<GraphToken> GetProjectTokens(IEnumerable<ProjectNode> projectNodes)
        {
            Inspection<ProjectNode, (ProjectNode, DependencyKind, DependencyOrigin)> projectInspection = new();
            foreach (var projectNode in projectNodes)
            {
                foreach (var (dependencyProject, dependencyKind, dependencyOrigin) in GetProjectDependencies(projectNode, projectInspection))
                {
                    if (projectNode.HasSdk || dependencyKind == DependencyKind.Direct || dependencyOrigin == DependencyOrigin.Package)
                    {
                        GraphLinkToken token = new(projectNode.Key, dependencyProject.Key, RestoreLabels.ProjectDependency);
                        token.Mark(RestoreLabels.Kind, dependencyKind.ToString());
                        yield return token;
                    }
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
                        foreach (var projectDependency in GetProjectDependencies(packagableProject, projectInspection))
                            yield return projectDependency with { Kind = DependencyKind.Transitive, Origin = DependencyOrigin.Package };
                    }
                }
            }
        }


        private IEnumerable<GraphToken> GetPackageTokens(IEnumerable<PackageNode> packageNodes, IEnumerable<ProjectNode> projectNodes)
        {
            Inspection<PackageNode, (PackageNode, DependencyKind, DependencyOrigin)> packageInspection = new();
            foreach (var packageNode in packageNodes)
            {
                foreach (var (dependencyPackage, dependencyKind, _) in GetPackageDependencies(packageNode, packageInspection))
                {
                    GraphLinkToken token = new(packageNode.Key, dependencyPackage.Key, RestoreLabels.PackageDependency);
                    token.Mark(RestoreLabels.Kind, dependencyKind.ToString());
                    yield return token;
                }
            }
            
            Inspection<ProjectNode, (PackageNode, DependencyKind, DependencyOrigin)> projectInspection = new();
            foreach (var projectNode in projectNodes)
            {
                foreach (var (dependencyPackage, dependencyKind, _) in GetPackageDependencies(projectNode, projectInspection))
                {
                    GraphLinkToken token = new(projectNode.Key, dependencyPackage.Key, RestoreLabels.PackageDependency);
                    token.Mark(RestoreLabels.Kind, dependencyKind.ToString());
                    yield return token;
                }
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

                    foreach (var packageDependency in GetPackageDependencies(referencedPackage, packageInspection))
                        yield return packageDependency with { Kind = DependencyKind.Transitive };
                }
            }
        }

        private IEnumerable<(PackageNode Package, DependencyKind Kind, DependencyOrigin Origin)> GetPackageDependencies(ProjectNode projectNode,
            Inspection<ProjectNode, (PackageNode, DependencyKind, DependencyOrigin)> packageInspection)
        {
            return packageInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<(PackageNode, DependencyKind, DependencyOrigin)> visitProject()
            {
                foreach (var referencedPackage in projectNode.ReferencedPackages().ToList())
                {
                    if (referencedPackage.Project() is null)
                    {
                        yield return new(referencedPackage, DependencyKind.Direct, DependencyOrigin.Package);

                        foreach (var dependencyPackage in referencedPackage.RestoredPackages().ToList())
                            yield return new(dependencyPackage, DependencyKind.Transitive, DependencyOrigin.Package);
                    }
                }
            }
        }


        
        private IEnumerable<GraphToken> GetAssemblyTokens(IEnumerable<PackageNode> packageNodes, IEnumerable<ProjectNode> projectNodes)
        {
            Inspection<PackageNode, (AssemblyNode, DependencyKind, DependencyOrigin)> packageInspection = new();
            foreach (var packageNode in packageNodes)
            {
                foreach (var (dependencyPackage, dependencyKind, _) in GetPackageDependencies(packageNode, packageInspection))
                {
                    GraphLinkToken token = new(packageNode.Key, dependencyPackage.Key, RestoreLabels.PackageDependency);
                    token.Mark(RestoreLabels.Kind, dependencyKind.ToString());
                    yield return token;
                }
            }
            
            Inspection<ProjectNode, (PackageNode, DependencyKind, DependencyOrigin)> projectInspection = new();
            foreach (var projectNode in projectNodes)
            {
                foreach (var (dependencyPackage, dependencyKind, _) in GetPackageDependencies(projectNode, projectInspection))
                {
                    GraphLinkToken token = new(projectNode.Key, dependencyPackage.Key, RestoreLabels.PackageDependency);
                    token.Mark(RestoreLabels.Kind, dependencyKind.ToString());
                    yield return token;
                }
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

                    foreach (var packageDependency in GetPackageDependencies(referencedPackage, packageInspection))
                        yield return packageDependency with { Kind = DependencyKind.Transitive };
                }
            }
        }

        private IEnumerable<(PackageNode Package, DependencyKind Kind, DependencyOrigin Origin)> GetPackageDependencies(ProjectNode projectNode,
            Inspection<ProjectNode, (PackageNode, DependencyKind, DependencyOrigin)> packageInspection)
        {
            return packageInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<(PackageNode, DependencyKind, DependencyOrigin)> visitProject()
            {
                foreach (var referencedPackage in projectNode.ReferencedPackages().ToList())
                {
                    if (referencedPackage.Project() is null)
                    {
                        yield return new(referencedPackage, DependencyKind.Direct, DependencyOrigin.Package);

                        foreach (var dependencyPackage in referencedPackage.RestoredPackages().ToList())
                            yield return new(dependencyPackage, DependencyKind.Transitive, DependencyOrigin.Package);
                    }
                }
            }
        }
    }
}
