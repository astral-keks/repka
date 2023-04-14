using Repka.Assemblies;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Frameworks;
using Repka.Paths;
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
            Inspection<PackageNode, AbsolutePath> packageInspection = new();
            IEnumerable<GraphToken> packageTokens = packageNodes
                .Peek(packageProgress.Increment)
                .SelectMany(packageNode => GetAssemblyTokens(GetPackageAssemblies(packageNode, packageInspection), packageNode))
                .ToList();
            foreach (var token in packageTokens)
                graph.Add(token);
            packageProgress.Complete();

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Resolving project assemblies", projectNodes.Count);
            Inspection<ProjectNode, AbsolutePath> projectInspection = new();
            IEnumerable<GraphToken> projectTokens = projectNodes
                .Peek(projectProgress.Increment)
                .SelectMany(projectNode => GetAssemblyTokens(GetProjectAssemblies(projectNode, projectInspection), projectNode))
                .ToList();
            foreach (var token in projectTokens)
                graph.Add(token);
            projectProgress.Complete();
        }

        private IEnumerable<GraphToken> GetAssemblyTokens(IEnumerable<AbsolutePath> assemblyPaths, GraphNode graphNode)
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                GraphKey assemblyKey = new(assemblyPath);
                yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                yield return new GraphLinkToken(graphNode.Key, assemblyKey, AssemblyLabels.Restored);
            }
        }

        private IEnumerable<AbsolutePath> GetPackageAssemblies(PackageNode packageNode, 
            Inspection<PackageNode, AbsolutePath> packageInspection)
        {
            return packageInspection.InspectOrGet(packageNode, () => visitPackage().ToHashSet());
            IEnumerable<AbsolutePath> visitPackage()
            {
                foreach (var assemblyPath in packageNode.AssemblyAssets(TargetFramework))
                    yield return assemblyPath;

                foreach (var assemblyName in packageNode.AssemblyReferences(TargetFramework))
                {
                    AssemblyMetadata? assembly = TargetFramework.Resolver.FindAssembly(assemblyName);
                    if (assembly is not null)
                        yield return new AbsolutePath(assembly.Location);
                }

                foreach (var dependencyPackage in packageNode.DependencyPackages(TargetFramework))
                {
                    foreach (var assemblyPath in GetPackageAssemblies(dependencyPackage, packageInspection))
                        yield return assemblyPath;
                }
            }
        }

        private IEnumerable<AbsolutePath> GetProjectAssemblies(ProjectNode projectNode,
            Inspection<ProjectNode, AbsolutePath> projectInspection)
        {
            return projectInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<AbsolutePath> visitProject()
            {
                foreach (var assembly in TargetFramework.Assemblies)
                    yield return new AbsolutePath(assembly.Location);

                foreach (var assemblyPath in projectNode.LibraryReferences)
                    yield return assemblyPath;

                foreach (var assemblyName in projectNode.AssemblyReferences)
                {
                    AssemblyMetadata? assembly = TargetFramework.Resolver.FindAssembly(assemblyName);
                    if (assembly is not null)
                        yield return new AbsolutePath(assembly.Location);
                }

                foreach (var dependencyPackage in projectNode.DependencyPackages())
                {
                    foreach (var assemblyNode in dependencyPackage.RestoredAssemblies())
                        yield return assemblyNode.Location;
                }

                foreach (var dependencyProject in projectNode.DependencyProjects())
                {
                    foreach (var assemblyPath in GetProjectAssemblies(dependencyProject, projectInspection))
                        yield return assemblyPath;
                }
            }
        }
    }
}
