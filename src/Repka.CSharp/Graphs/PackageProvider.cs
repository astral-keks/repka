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
        public NuGetManager NuGet { get; init; } = new(".");
        public FrameworkDefinition Framework { get; init; } = FrameworkDefinitions.Current;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                List<ProjectNode> projectNodes = graph.Projects().ToList();
                ProgressPercentage packageProgress = Progress.Percent("Resolving packages", projectNodes.Count);
                foreach (var token in GetPackageTokens(projectNodes.Peek(packageProgress.Increment)))
                    graph.Add(token);
                packageProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetPackageTokens(IEnumerable<ProjectNode> projectNodes)
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
                    .Select(packageReference => NuGet.ResolvePackage(packageReference))
                    .ToList();
                foreach (var packageDependency in packageDependencies)
                {
                    PackageKey packageDependencyKey = new(packageDependency);
                    yield return new GraphLinkToken(projectNode.Key, packageDependencyKey, PackageLabels.PackageDependency);
                    
                    foreach (var token in GetPackageTokens(packageDependency, packageIdsFromProjects, packageTraversal))
                        yield return token;
                }
            }
        }

        private ICollection<GraphToken> GetPackageTokens(NuGetDescriptor packageDescriptor, HashSet<NuGetIdentifier> packageIdsFromProjects, 
            GraphTraversal<NuGetDescriptor, GraphToken> packageTraversal)
        {
            return packageTraversal.Visit(packageDescriptor, () => packageVisitor().ToList());
            
            IEnumerable<GraphToken> packageVisitor()
            {
                NuGetPackage? package = NuGet.RestorePackage(packageDescriptor);
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

                        AssemblyFile? frameworkAssembly = Framework.ResolveAssembly(frameworkReference.Target);
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
                            NuGetDescriptor packageDependency = NuGet.DiscoverPackage(packageReference.Target);
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
                        foreach (var token in GetPackageTokens(packageDependency, packageIdsFromProjects, packageTraversal))
                            yield return token;
                    }
                }
            }
        }
    }
}
