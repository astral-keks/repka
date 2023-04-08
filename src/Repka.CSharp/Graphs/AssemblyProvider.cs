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
        public FrameworkDefinition TargetFramework { get; init; } = FrameworkDefinitions.Current;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            List<PackageNode> packageNodes = graph.Packages().ToList();
            ProgressPercentage packageProgress = Progress.Percent("Resolving package assemblies", packageNodes.Count);
            GraphTraversal<PackageNode, AssemblyDescriptor> packageTraversal = new() { Strategy = GraphTraversalStrategy.RecallHistory };
            IEnumerable<GraphToken> packageTokens = packageNodes.AsParallel(8)
                .Peek(packageProgress.Increment)
                .SelectMany(packageNode => GetAssemblyTokens(GetPackageAssemblies(packageNode, packageTraversal), packageNode))
                .ToList();
            foreach (var token in packageTokens)
                graph.Add(token);
            packageProgress.Complete();

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Resolving project assemblies", projectNodes.Count);
            GraphTraversal<ProjectNode, AssemblyDescriptor> projectTraversal = new() { Strategy = GraphTraversalStrategy.RecallHistory };
            IEnumerable<GraphToken> projectTokens = projectNodes.AsParallel(8)
                .Peek(projectProgress.Increment)
                .SelectMany(projectNode => GetAssemblyTokens(GetProjectAssemblies(projectNode, projectTraversal), projectNode))
                .ToList();
            foreach (var token in projectTokens)
                graph.Add(token);
            projectProgress.Complete();
        }

        private IEnumerable<GraphToken> GetAssemblyTokens(IEnumerable<AssemblyDescriptor> assemblies, GraphNode graphNode)
        {
            foreach (var assembly in assemblies)
            {
                GraphKey assemblyKey = new(assembly.Location);
                yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                yield return new GraphLinkToken(graphNode.Key, assemblyKey, AssemblyLabels.AssemblyDependency);
            }
        }

        private IEnumerable<AssemblyDescriptor> GetPackageAssemblies(PackageNode packageNode, 
            GraphTraversal<PackageNode, AssemblyDescriptor> packageTraversal)
        {
            return packageTraversal.Visit(packageNode, () => visitPackage().ToHashSet());
            IEnumerable<AssemblyDescriptor> visitPackage()
            {
                foreach (var assembly in packageNode.Assemblies(TargetFramework))
                    yield return assembly;

                foreach (var frameworkReference in packageNode.FrameworkReferences(TargetFramework))
                {
                    AssemblyDescriptor? frameworkAssembly = TargetFramework.Resolver.FindAssembly(frameworkReference.AssemblyName);
                    if (frameworkAssembly is not null)
                        yield return frameworkAssembly;
                }

                foreach (var packageDependency in packageNode.PackageDependencies(TargetFramework))
                {
                    foreach (var assembly in GetPackageAssemblies(packageDependency, packageTraversal))
                        yield return assembly;
                }
            }
        }

        private IEnumerable<AssemblyDescriptor> GetProjectAssemblies(ProjectNode projectNode,
            GraphTraversal<ProjectNode, AssemblyDescriptor> projectTraversal)
        {
            return projectTraversal.Visit(projectNode, () => visitProject().ToHashSet());
            IEnumerable<AssemblyDescriptor> visitProject()
            {
                foreach (var assembly in TargetFramework.Assemblies)
                    yield return assembly;

                foreach (var assembly in projectNode.LibraryDependencies)
                    yield return assembly;

                foreach (var frameworkReference in projectNode.FrameworkReferences)
                {
                    AssemblyDescriptor? frameworkAssembly = TargetFramework.Resolver.FindAssembly(frameworkReference);
                    if (frameworkAssembly is not null)
                        yield return frameworkAssembly;
                }

                foreach (var packageDependency in projectNode.PackageDependencies(TargetFramework))
                {
                    foreach (var assembly in packageDependency.AssemblyDependencies())
                        yield return assembly.Descriptor;
                }

                foreach (var projectDependency in projectNode.ProjectDependencies)
                {
                    foreach (var assembly in GetProjectAssemblies(projectDependency, projectTraversal))
                        yield return assembly;
                }
            }
        }
    }
}
