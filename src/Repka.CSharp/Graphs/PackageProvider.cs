using NuGet.Packaging.Core;
using Repka.Assemblies;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Frameworks;
using Repka.Packaging;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class PackageProvider : GraphProvider
    {
        public NuGetProvider NuGetProvider { get; init; } = new();
        public FrameworkProvider FrameworkProvider { get; init; } = new();

        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                NuGetManager packageManager = NuGetProvider.GetManager(directory.FullName);
                FrameworkDirectory frameworkDirectory = FrameworkProvider.GetFrameworkDirectory();

                List<ProjectNode> projectNodes = graph.Projects().ToList();
                ProgressPercentage packageProgress = Progress.Percent("Resolving packages", projectNodes.Count);
                foreach (var token in GetPackageTokens(projectNodes.Peek(packageProgress.Increment), packageManager, frameworkDirectory))
                    yield return token;
                packageProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetPackageTokens(IEnumerable<ProjectNode> projectNodes, 
            NuGetManager packageManager, FrameworkDirectory frameworkDirectory)
        {
            HashSet<NuGetIdentifier> packageIdsFromProjects = projectNodes
                .Select(projectNode => projectNode.PackageId)
                .OfType<NuGetIdentifier>()
                .ToHashSet();

            GraphTraversal<NuGetDescriptor, GraphToken> packageTraversal = new() { Strategy = GraphTraversalStrategy.BypassHistory };
            foreach (var projectNode in projectNodes)
            {
                if (projectNode.PackageId is not null)
                {
                    PackageKey packageKey = new(projectNode.PackageId.ToString(), null);
                    yield return new GraphNodeToken(packageKey, PackageLabels.Package);
                }

                IEnumerable<NuGetDescriptor> packageDependencies = projectNode.PackageReferences
                    .Where(packageReference => !packageIdsFromProjects.Contains(packageReference.Id))
                    .Select(packageReference => packageManager.ResolvePackage(packageReference))
                    .ToList();
                foreach (var packageDependency in packageDependencies)
                {
                    PackageKey packageDependencyKey = new(packageDependency);
                    yield return new GraphLinkToken(projectNode.Key, packageDependencyKey, PackageLabels.PackageDependency);
                    
                    var tokens = GetPackageTokens(packageDependency, packageManager, frameworkDirectory, packageIdsFromProjects, packageTraversal);
                    foreach (var token in tokens)
                        yield return token;
                }
            }
        }

        private ICollection<GraphToken> GetPackageTokens(NuGetDescriptor packageDescriptor, NuGetManager packageManager, FrameworkDirectory frameworkDirectory,
            HashSet<NuGetIdentifier> packageIdsFromProjects, GraphTraversal<NuGetDescriptor, GraphToken> packageTraversal)
        {
            return packageTraversal.Visit(packageDescriptor, () => packageVisitor().ToList());
            
            IEnumerable<GraphToken> packageVisitor()
            {
                NuGetPackage? package = packageManager.RestorePackage(packageDescriptor);
                if (package is not null)
                {
                    PackageKey packageKey = new(package);
                    yield return new GraphNodeToken(packageKey, PackageLabels.Package);

                    foreach (var assembly in package.Assemblies)
                        yield return new GraphLinkToken(packageKey, assembly.Target ?? GraphKey.Null, PackageLabels.PackageAssembly)
                            .Label(assembly.Framework.Moniker());

                    foreach (var frameworkReference in package.FrameworkReferences)
                    {
                        yield return new GraphLinkToken(packageKey, frameworkReference.Target ?? GraphKey.Null, PackageLabels.FrameworkReference)
                            .Label(frameworkReference.Framework.Moniker());

                        AssemblyFile? frameworkAssembly = frameworkDirectory.ResolveAssembly(frameworkReference.Target);
                        yield return new GraphLinkToken(packageKey, frameworkAssembly?.Path ?? GraphKey.Null, PackageLabels.FrameworkDependency)
                            .Label(frameworkReference.Framework.Moniker());
                    }

                    HashSet<NuGetDescriptor> packageDependencies = new();
                    foreach (var packageReference in package.PackageReferences)
                    {
                        PackageKey? packageReferenceKey = default;
                        PackageKey? packageDependencyKey = default;
                        if (packageReference.Target is not null && !packageIdsFromProjects.Contains(packageReference.Target.Id))
                        {
                            NuGetDescriptor packageDependency = packageManager.DiscoverPackage(packageReference.Target);
                            packageDependencies.Add(packageDependency);

                            packageReferenceKey = new(packageReference.Target);
                            packageDependencyKey = new(packageDependency);
                        }

                        if (packageReferenceKey is not null)
                            yield return new GraphLinkToken(packageKey, packageReferenceKey ?? GraphKey.Null, PackageLabels.PackageReference)
                                .Label(packageReference.Framework.Moniker());

                        yield return new GraphLinkToken(packageKey, packageDependencyKey ?? GraphKey.Null, PackageLabels.PackageDependency)
                            .Label(packageReference.Framework.Moniker());
                    }

                    foreach (var packageDependency in packageDependencies)
                    {
                        var tokens = GetPackageTokens(packageDependency, packageManager, frameworkDirectory, 
                            packageIdsFromProjects, packageTraversal);
                        foreach (var token in tokens)
                            yield return token;
                    }
                }
            }
        }
    }
}
