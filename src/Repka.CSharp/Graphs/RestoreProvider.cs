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

            ProgressPercentage assemblyProgress = Progress.Percent("Restoring assemblies", packageNodes.Count + projectNodes.Count);
            IEnumerable<GraphToken> assemblyTokens = GetAssemblyTokens(packageNodes.Peek(assemblyProgress.Increment),
                projectNodes.Peek(assemblyProgress.Increment));
            foreach (var token in assemblyTokens)
                graph.Add(token);
            assemblyProgress.Complete();
        }

        #region Projects
        private IEnumerable<GraphToken> GetProjectTokens(IEnumerable<ProjectNode> projectNodes)
        {
            Inspection<ProjectNode, (ProjectNode, DependencyKind, DependencyOrigin)> projectInspection = new();
            foreach (var projectNode in projectNodes)
            {
                foreach (var (dependencyProject, dependencyKind, dependencyOrigin) in RestoreProject(projectNode, projectInspection))
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

        private IEnumerable<(ProjectNode Project, DependencyKind Kind, DependencyOrigin Origin)> RestoreProject(ProjectNode projectNode,
            Inspection<ProjectNode, (ProjectNode, DependencyKind, DependencyOrigin)> projectInspection)
        {
            return projectInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<(ProjectNode, DependencyKind, DependencyOrigin)> visitProject()
            {
                foreach (var referencedProject in projectNode.ReferencedProjects().ToList())
                {
                    yield return new(referencedProject, DependencyKind.Direct, DependencyOrigin.Project);
                    foreach (var dependency in RestoreProject(referencedProject, projectInspection))
                        yield return dependency with { Kind = DependencyKind.Transitive };
                }

                foreach (var referencedPackage in projectNode.ReferencedPackages().ToList())
                {
                    ProjectNode? packagableProject = referencedPackage.Project();
                    if (packagableProject is not null)
                    {
                        yield return new(packagableProject, DependencyKind.Direct, DependencyOrigin.Package);
                        foreach (var projectDependency in RestoreProject(packagableProject, projectInspection))
                            yield return projectDependency with { Kind = DependencyKind.Transitive, Origin = DependencyOrigin.Package };
                    }
                }
            }
        }
        #endregion

        #region Packages
        private IEnumerable<GraphToken> GetPackageTokens(IEnumerable<PackageNode> packageNodes, IEnumerable<ProjectNode> projectNodes)
        {
            Inspection<PackageNode, PackageNode> packageInspection = new();
            foreach (var packageNode in packageNodes)
            {
                foreach (var dependencyPackage in RestorePackages(packageNode, packageInspection))
                {
                    GraphLinkToken token = new(packageNode.Key, dependencyPackage.Key, RestoreLabels.PackageDependency);
                    yield return token;
                }
            }

            Inspection<ProjectNode, PackageNode> projectInspection = new();
            foreach (var projectNode in projectNodes)
            {
                foreach (var dependencyPackage in RestorePackages(projectNode, projectInspection))
                {
                    GraphLinkToken token = new(projectNode.Key, dependencyPackage.Key, RestoreLabels.PackageDependency);
                    yield return token;
                }
            }
        }

        private ICollection<PackageNode> RestorePackages(PackageNode packageNode,
            Inspection<PackageNode, PackageNode> packageInspection)
        {
            return packageNode.ToOptional()
                .Recurse(packageNode => packageNode.ReferencedPackages(TargetFramework))
                .Flatten()
                .SelectMany(packageNode => packageInspection.InspectOrGet(packageNode, () => visitPackage(packageNode).ToHashSet()))
                .ToHashSet();
            IEnumerable<PackageNode> visitPackage(PackageNode packageNode)
            {
                foreach (var referencedPackage in packageNode.ReferencedPackages(TargetFramework).ToList())
                    yield return referencedPackage;
            }
        }

        private ICollection<PackageNode> RestorePackages(ProjectNode projectNode,
            Inspection<ProjectNode, PackageNode> projectInspection)
        {
            return projectNode.ToOptional()
                .Recurse(projectNode => projectNode.ReferencedProjects())
                .Flatten()
                .SelectMany(projectNode => projectInspection.InspectOrGet(projectNode, () => visitProject(projectNode).ToHashSet()))
                .ToHashSet();
            IEnumerable<PackageNode> visitProject(ProjectNode projectNode)
            {
                foreach (var referencedPackage in projectNode.ReferencedPackages().ToList())
                {
                    if (referencedPackage.Project() is null)
                        yield return referencedPackage;

                    foreach (var dependencyPackage in referencedPackage.RestoredPackages().ToList())
                        yield return dependencyPackage;
                }
            }
        }
        #endregion

        #region Assemblies
        private IEnumerable<GraphToken> GetAssemblyTokens(IEnumerable<PackageNode> packageNodes, IEnumerable<ProjectNode> projectNodes)
        {
            Inspection<PackageNode, AssemblyNode> packageInspection = new();
            foreach (var packageNode in packageNodes)
            {
                foreach (var dependencyPackage in RestoreAssemblies(packageNode, packageInspection))
                {
                    GraphLinkToken token = new(packageNode.Key, dependencyPackage.Key, RestoreLabels.AssemblyDependency);
                    yield return token;
                }
            }

            Inspection<ProjectNode, AssemblyNode> projectInspection = new();
            foreach (var projectNode in projectNodes)
            {
                foreach (var dependencyPackage in RestoreAssemblies(projectNode, projectInspection).Distinct())
                {
                    GraphLinkToken token = new(projectNode.Key, dependencyPackage.Key, RestoreLabels.AssemblyDependency);
                    yield return token;
                }
            }
        }

        private ICollection<AssemblyNode> RestoreAssemblies(PackageNode packageNode,
            Inspection<PackageNode, AssemblyNode> packageInspection)
        {
            return packageNode.RestoredPackages().Prepend(packageNode)
                .SelectMany(packageNode => packageInspection.InspectOrGet(packageNode, () => visitPackage(packageNode).ToHashSet()))
                .ToHashSet();
            IEnumerable<AssemblyNode> visitPackage(PackageNode packageNode)
            {
                foreach (var assembly in packageNode.AssetAssemblies())
                    yield return assembly;

                foreach (var assembly in packageNode.ReferencedFrameworkAssemblies())
                    yield return assembly;
            }
        }

        private ICollection<AssemblyNode> RestoreAssemblies(ProjectNode projectNode, 
            Inspection<ProjectNode, AssemblyNode> projectInspection)
        {
            return projectNode.RestoredProjects().Prepend(projectNode)
                .SelectMany(projectNode => projectInspection.InspectOrGet(projectNode, () => visitProject(projectNode).ToHashSet()))
                .ToHashSet();
            IEnumerable<AssemblyNode> visitProject(ProjectNode projectNode)
            {
                foreach (var assembly in projectNode.ReferencedLibraries())
                    yield return assembly;
                
                foreach (var assembly in projectNode.ReferencedFrameworkAssemblies())
                    yield return assembly;

                foreach (var restoredPackage in projectNode.RestoredPackages())
                {
                    foreach (var restoredAssembly in restoredPackage.RestoredAssemblies())
                        yield return restoredAssembly;
                }
            }
        } 
        #endregion
    }
}
