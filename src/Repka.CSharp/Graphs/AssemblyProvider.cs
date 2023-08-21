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
            IEnumerable<GraphToken> packageTokens = GetPackageAssemblyTokens(packageNodes.Peek(packageProgress.Increment))
                .ToList();
            foreach (var token in packageTokens)
                graph.Add(token);
            packageProgress.Complete();

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Resolving project assemblies", projectNodes.Count);
            IEnumerable<GraphToken> projectTokens = GetProjectAssemblyTokens(projectNodes.Peek(projectProgress.Increment))
                .ToList();
            foreach (var token in projectTokens)
                graph.Add(token);
            projectProgress.Complete();
        }

        private IEnumerable<GraphToken> GetPackageAssemblyTokens(IEnumerable<PackageNode> packageNodes)
        {
            foreach (var packageNode in packageNodes)
            {
                foreach (var assemblyPath in packageNode.AssemblyAssets(TargetFramework))
                {
                    GraphKey assemblyKey = new(assemblyPath);
                    yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                    yield return new GraphLinkToken(packageNode.Key, assemblyKey, AssemblyLabels.AssemblyReference);
                }

                foreach (var assemblyName in packageNode.FrameworkAssemblyReferences(TargetFramework))
                {
                    AssemblyMetadata? assembly = TargetFramework.Resolver.FindAssembly(assemblyName);
                    if (assembly is not null)
                    {
                        GraphKey assemblyKey = new(assembly.Location);
                        yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                        yield return new GraphLinkToken(packageNode.Key, assemblyKey, AssemblyLabels.AssemblyReference);
                        yield return new GraphLinkToken(packageNode.Key, assemblyKey, AssemblyLabels.FrameworkAssemblyReference);
                    }
                }
            }
        }

        private IEnumerable<GraphToken> GetProjectAssemblyTokens(IEnumerable<ProjectNode> projectNodes)
        {
            foreach (var projectNode in projectNodes)
            {
                foreach (var assembly in TargetFramework.Assemblies)
                {
                    GraphKey assemblyKey = new(assembly.Location);
                    yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                    yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.AssemblyReference);
                    yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.FrameworkAssemblyReference);
                }

                foreach (var assemblyName in projectNode.FrameworkAssemblyReferences)
                {
                    AssemblyMetadata? assembly = TargetFramework.Resolver.FindAssembly(assemblyName);
                    if (assembly is not null)
                    {
                        GraphKey assemblyKey = new(assembly.Location);
                        yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                        yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.AssemblyReference);
                        yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.FrameworkAssemblyReference);
                    }
                }

                foreach (var assemblyPath in projectNode.LibraryReferences)
                {
                    GraphKey assemblyKey = new(assemblyPath);
                    yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                    yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.AssemblyReference);
                }
            }
        }
    }
}
