using Repka.Collections;
using Repka.Diagnostics;
using Repka.Packaging;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class PackageProvider : GraphProvider
    {
        public NuGetManager NuGetManager { get; init; } = new(".");

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                List<ProjectNode> projectNodes = graph.Projects().ToList();
                HashSet<NuGetIdentifier> packageIdsFromProjects = projectNodes
                    .Select(projectNode => projectNode.PackageId)
                    .OfType<NuGetIdentifier>()
                    .ToHashSet();
                Inspection<NuGetDescriptor, NuGetPackage> packageInspection = new();
                ProgressPercentage packageProgress = Progress.Percent("Resolving packages", projectNodes.Count);
                IEnumerable<GraphToken> packageTokens = projectNodes.AsParallel(8)
                    .Peek(packageProgress.Increment)
                    .SelectMany(projectNode => GetPackageTokens(projectNode, packageIdsFromProjects, packageInspection))
                    .ToList();
                foreach (var token in packageTokens)
                    graph.Add(token);
                packageProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetPackageTokens(ProjectNode projectNode, HashSet<NuGetIdentifier> packageIdsFromProjects,
            Inspection<NuGetDescriptor, NuGetPackage> packageInspection)
        {
            if (projectNode.PackageId is not null)
            {
                NuGetDescriptor packageDescriptor = new(projectNode.PackageId, null);
                GraphKey packageKey = new(packageDescriptor.ToString());
                yield return new GraphNodeToken(packageKey, PackageLabels.Package);
            }

            IEnumerable<NuGetDescriptor> packageDependencies = projectNode.PackageReferences
                //.Where(packageReference => !packageIdsFromProjects.Contains(packageReference.Id))
                .Select(packageReference => NuGetManager.ResolvePackage(packageReference))
                .ToList();
            foreach (var packageDependency in packageDependencies)
            {
                GraphKey packageDependencyKey = new(packageDependency.ToString());
                yield return new GraphLinkToken(projectNode.Key, packageDependencyKey, PackageLabels.ReferencedPackage);

                foreach (var token in GetPackageTokens(packageDependency, packageIdsFromProjects, packageInspection))
                    yield return token;
            }
        }

        private IEnumerable<GraphToken> GetPackageTokens(NuGetDescriptor packageDescriptor, HashSet<NuGetIdentifier> packageIdsFromProjects,
            Inspection<NuGetDescriptor, NuGetPackage> packageInspection)
        {
            foreach (var package in GetPackageDependencies(packageDescriptor, packageIdsFromProjects, packageInspection))
            {
                GraphKey packageKey = new(package.Descriptor.ToString());
                yield return new GraphNodeToken(packageKey, PackageLabels.Package);

                foreach (var assembly in package.Assemblies)
                {
                    GraphKey assemblyKey = new(assembly.Locaton ?? GraphKey.Null);
                    yield return new GraphLinkToken(packageKey, assemblyKey, PackageLabels.AssemblyAsset)
                        .Mark(assembly.Framework.ToMoniker());
                }

                foreach (var assemblyReference in package.AssemblyReferences)
                {
                    GraphKey assemblyReferenceKey = new(assemblyReference.AssemblyName ?? GraphKey.Null);
                    yield return new GraphLinkToken(packageKey, assemblyReferenceKey, PackageLabels.AssemblyReference)
                        .Mark(assemblyReference.Framework.ToMoniker());
                }

                foreach (var packageReference in package.PackageReferences)
                {
                    GraphKey packageDependencyKey = GraphKey.Null;
                    if (packageReference.Descriptor is not null && !packageIdsFromProjects.Contains(packageReference.Descriptor.Id))
                    {
                        GraphKey packageReferenceKey = new(packageReference.Descriptor.ToString());
                        yield return new GraphLinkToken(packageKey, packageReferenceKey, PackageLabels.PackageReference)
                            .Mark(packageReference.Framework.ToMoniker());

                        NuGetDescriptor packageDependency = NuGetManager.DiscoverPackage(packageReference.Descriptor);
                        packageDependencyKey = new(packageDependency.ToString());
                    }

                    yield return new GraphLinkToken(packageKey, packageDependencyKey, PackageLabels.ReferencedPackage)
                        .Mark(packageReference.Framework.ToMoniker());
                }
            }
        }

        private ICollection<NuGetPackage> GetPackageDependencies(NuGetDescriptor packageDescriptor, HashSet<NuGetIdentifier> packageIdsFromProjects,
            Inspection<NuGetDescriptor, NuGetPackage> packageInspection)
        {
            return packageInspection.InspectOrIgnore(packageDescriptor, () => visitPackage().ToList());
            IEnumerable<NuGetPackage> visitPackage()
            {
                NuGetPackage? package = NuGetManager.RestorePackage(packageDescriptor);
                if (package is not null)
                {
                    yield return package;

                    foreach (var packageReference in package.PackageReferences.Select(packageReference => packageReference.Descriptor).Distinct())
                    {
                        if (packageReference is not null && !packageIdsFromProjects.Contains(packageReference.Id))
                        {
                            foreach (var packageDependency in GetPackageDependencies(packageReference, packageIdsFromProjects, packageInspection))
                                yield return packageDependency;
                        }
                    }
                }
            }
        }
    }
}
