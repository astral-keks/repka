using Repka.Assemblies;
using Repka.Collections;
using Repka.Diagnostics;
using static Repka.Graphs.AssemblyDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class AssemblyProvider : GraphProvider
    {
        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage packageProgress = Progress.Percent("Resolving assemblies", projectNodes.Count);
            foreach (var token in GetAssemblyTokens(projectNodes.Peek(packageProgress.Increment)))
                yield return token;
            packageProgress.Complete();
        }

        private IEnumerable<GraphToken> GetAssemblyTokens(IEnumerable<ProjectNode> projectNodes)
        {
            foreach (ProjectNode projectNode in projectNodes) 
            {
                List<ProjectNode> projectDependencies = projectNode.ProjectDependencies.Traverse().ToList();
                HashSet<AssemblyFile> projectAssemblies = GetProjectAssemblies(projectNode, projectDependencies);
                HashSet<AssemblyFile> packageAssemblies = GetPackageAssemblies(projectNode, projectDependencies);

                foreach (var assembly in projectAssemblies.Union(packageAssemblies))
                {
                    GraphKey assemblyKey = new(assembly.Path);
                    yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                    yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.AssemblyDependency);
                }
            }
        }

        private HashSet<AssemblyFile> GetProjectAssemblies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            HashSet<AssemblyFile> projectAssemblies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => Enumerable.Concat(
                    projectNode.FrameworkDependencies,
                    projectNode.LibraryDependencies))
                .ToHashSet();

            return projectAssemblies;
        }

        private HashSet<AssemblyFile> GetPackageAssemblies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            string? targetFramework = projectNode.TargetFramework;

            List<PackageNode> packageDependencies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => projectNode.PackageDependencies(targetFramework).Traverse())
                .Distinct()
                .GroupBy(packageNode => packageNode.Id)
                .Select(packageGroup => packageGroup.MaxBy(packageNode => packageNode.Version))
                .OfType<PackageNode>()
                .ToList();
            HashSet<AssemblyFile> packageAssemblies = packageDependencies
                .SelectMany(packageNode => Enumerable.Concat(
                    packageNode.PackageAssemblies(targetFramework),
                    packageNode.FrameworkDependencies(targetFramework)))
                .ToHashSet();

            return packageAssemblies;
        }
    }
}
