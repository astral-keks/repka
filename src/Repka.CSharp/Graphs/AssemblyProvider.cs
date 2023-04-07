﻿using Repka.Assemblies;
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
            foreach (var token in GetAssemblyTokens(projectNodes.Peek(packageProgress.Increment)))
                graph.Add(token);
            packageProgress.Complete();
        }

        private IEnumerable<GraphToken> GetAssemblyTokens(IEnumerable<ProjectNode> projectNodes)
        {
            foreach (ProjectNode projectNode in projectNodes) 
            {
                List<ProjectNode> projectDependencies = projectNode.ProjectDependencies.Traverse().ToList();
                HashSet<AssemblyDescriptor> projectAssemblies = GetProjectAssemblies(projectNode, projectDependencies);
                HashSet<AssemblyDescriptor> packageAssemblies = GetPackageAssemblies(projectNode, projectDependencies);

                foreach (var assembly in projectAssemblies.Union(packageAssemblies))
                {
                    GraphKey assemblyKey = new(assembly.Location);
                    yield return new GraphNodeToken(assemblyKey, AssemblyLabels.Assembly);
                    yield return new GraphLinkToken(projectNode.Key, assemblyKey, AssemblyLabels.AssemblyDependency);
                }
            }
        }

        private HashSet<AssemblyDescriptor> GetProjectAssemblies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            HashSet<AssemblyDescriptor> projectAssemblies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => Framework.Assemblies
                    .Concat(projectNode.FrameworkReferences.Select(Framework.Resolver.FindAssembly).OfType<AssemblyDescriptor>())
                    .Concat(projectNode.LibraryDependencies))
                .ToHashSet();

            return projectAssemblies;
        }

        private HashSet<AssemblyDescriptor> GetPackageAssemblies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            string? targetFramework = Framework.Moniker;

            List<PackageNode> packageDependencies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => projectNode.PackageDependencies(targetFramework).Traverse())
                .Distinct()
                .GroupBy(packageNode => packageNode.Id)
                .Select(packageGroup => packageGroup.MaxBy(packageNode => packageNode.Version))
                .OfType<PackageNode>()
                .ToList();
            HashSet<AssemblyDescriptor> packageAssemblies = packageDependencies
                .SelectMany(packageNode => Enumerable.Concat(
                    packageNode.Assemblies(targetFramework),
                    packageNode.FrameworkReferences(targetFramework)
                        .Select(frameworkReference => Framework.Resolver.FindAssembly(frameworkReference.AssemblyName))
                        .OfType<AssemblyDescriptor>()))
                .ToHashSet();

            return packageAssemblies;
        }
    }
}
