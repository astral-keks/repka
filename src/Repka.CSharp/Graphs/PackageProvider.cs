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
                .Where(packageReference => !packageIdsFromProjects.Contains(packageReference.Id))
                .Select(packageReference => NuGetManager.ResolvePackage(packageReference))
                .ToList();
            foreach (var packageDependency in packageDependencies)
            {
                GraphKey packageDependencyKey = new(packageDependency.ToString());
                yield return new GraphLinkToken(projectNode.Key, packageDependencyKey, PackageLabels.PackageDependency);

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
                    yield return new GraphLinkToken(packageKey, assembly.Locaton ?? GraphKey.Null, PackageLabels.PackageAssembly)
                        .Label(assembly.Framework.ToMoniker());

                foreach (var frameworkReference in package.FrameworkReferences)
                {
                    yield return new GraphLinkToken(packageKey, frameworkReference.AssemblyName ?? GraphKey.Null, PackageLabels.PackageFrameworkReference)
                        .Label(frameworkReference.Framework.ToMoniker());
                }

                foreach (var packageReference in package.PackageReferences)
                {
                    GraphKey? packageDependencyKey = default;
                    if (packageReference.Descriptor is not null && !packageIdsFromProjects.Contains(packageReference.Descriptor.Id))
                    {
                        GraphKey packageReferenceKey = new(packageReference.Descriptor.ToString());
                        yield return new GraphLinkToken(packageKey, packageReferenceKey, PackageLabels.PackageReference)
                            .Label(packageReference.Framework.ToMoniker());

                        NuGetDescriptor packageDependency = NuGetManager.DiscoverPackage(packageReference.Descriptor);
                        packageDependencyKey = new(packageDependency.ToString());
                    }

                    yield return new GraphLinkToken(packageKey, packageDependencyKey ?? GraphKey.Null, PackageLabels.PackageDependency)
                        .Label(packageReference.Framework.ToMoniker());
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
