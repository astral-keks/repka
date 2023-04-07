using Repka.Assemblies;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Frameworks;
using static Repka.Graphs.AssemblyDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class AssemblyProvider : GraphProvider
    {
        public FrameworkDefinition Framework { get; init; } = FrameworkDefinitions.Current;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage packageProgress = Progress.Percent("Resolving assemblies", projectNodes.Count);
            IEnumerable<GraphToken> tokens = projectNodes.AsParallel(8)
                .Peek(packageProgress.Increment)
                .SelectMany(projectNode => GetAssemblyTokens(projectNode))
                .ToList();
            foreach (var token in tokens)
                graph.Add(token);
            packageProgress.Complete();
        }

        private IEnumerable<GraphToken> GetAssemblyTokens(ProjectNode projectNode)
        {
            List<ProjectNode> projectDependencies = projectNode.ProjectDependencies.Traverse().ToList();
            List<AssemblyDescriptor> projectAssemblies = GetProjectAssemblies(projectNode, projectDependencies);
            List<AssemblyDescriptor> packageAssemblies = GetPackageAssemblies(projectNode, projectDependencies);

            foreach (var assembly in projectAssemblies.Union(packageAssemblies))
            {
                GraphKey assemblyKey = new(assembly.Location);
                yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.Assembly);
            }
        }

        private List<AssemblyDescriptor> GetProjectAssemblies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            List<AssemblyDescriptor> projectAssemblies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => Framework.Assemblies
                    .Concat(projectNode.FrameworkReferences.Select(Framework.Resolver.FindAssembly).OfType<AssemblyDescriptor>())
                    .Concat(projectNode.LibraryDependencies))
                .ToList();

            return projectAssemblies;
        }

        private List<AssemblyDescriptor> GetPackageAssemblies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            string? targetFramework = Framework.Moniker;

            List<PackageNode> packageDependencies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => projectNode.PackageDependencies(targetFramework).Traverse())
                .Distinct()
                .GroupBy(packageNode => packageNode.Id)
                .Select(packageGroup => packageGroup.MaxBy(packageNode => packageNode.Version))
                .OfType<PackageNode>()
                .ToList();
            List<AssemblyDescriptor> packageAssemblies = packageDependencies
                .SelectMany(packageNode => Enumerable.Concat(
                    packageNode.Assemblies(targetFramework),
                    packageNode.FrameworkReferences(targetFramework)
                        .Select(frameworkReference => Framework.Resolver.FindAssembly(frameworkReference.AssemblyName))
                        .OfType<AssemblyDescriptor>()))
                .ToList();

            return packageAssemblies;
        }
    }
}
