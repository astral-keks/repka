﻿using NuGet.Frameworks;
using NuGet.Versioning;
using Repka.Assemblies;
using Repka.Collections;
using Repka.Packaging;
using Repka.Paths;
using static Repka.Graphs.AssemblyDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class PackageDsl
    {
        public static IEnumerable<PackageNode> Packages(this Graph graph) => graph.Nodes()
            .Select(node => node.AsPackage()).OfType<PackageNode>();

        public static PackageNode? Package(this Graph graph, GraphKey key) => graph.Node(key).AsPackage();

        public static PackageNode? AsPackage(this GraphNode? node) => node?.Labels.Contains(PackageLabels.Package) == true
            ? new PackageNode(node)
            : default;

        public static PackageGrouping GroupByTargetFramework(this IEnumerable<GraphLink> links) => new(links);

        public class PackageNode : GraphNode
        {
            internal PackageNode(GraphNode node) : base(node)
            {
                Descriptor = NuGetDescriptor.Parse(Key);
            }

            public NuGetIdentifier Id => Descriptor.Id;

            public NuGetVersion? Version => Descriptor.Version;

            public NuGetDescriptor Descriptor { get; }


            public ProjectNode? Project() => Inputs(ProjectLabels.Package)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>()
                .FirstOrDefault();

            
            public IEnumerable<AbsolutePath> AssemblyAssets(string? targetFramework) => Outputs(PackageLabels.AssemblyAsset)
                .GroupByTargetFramework().SelectNearest(targetFramework)
                .Select(nearest => nearest.Link.TargetKey.AsAbsolutePath());

            
            public IEnumerable<AssemblyName> AssemblyReferences(string? targetFramework) => Outputs(PackageLabels.AssemblyReference)
                .GroupByTargetFramework().SelectNearest(targetFramework)
                .Select(nearest => nearest.Link.TargetKey.AsAssemblyName());

            
            public IEnumerable<ProjectNode> DependingProjects() => Inputs(PackageLabels.DependencyPackage)
                .Select(link => link.Source().AsProject()).OfType<ProjectNode>();


            public IEnumerable<NuGetDescriptor> PackageReferences(string? targetFramework) => Outputs(ProjectLabels.PackageReference)
                .GroupByTargetFramework().SelectNearest(targetFramework)
                .Select(nearest => NuGetDescriptor.Parse(nearest.Link.TargetKey));

            public IEnumerable<PackageNode> DependencyPackages(string? targetFramework) => Outputs(PackageLabels.DependencyPackage)
                .GroupByTargetFramework().SelectNearest(targetFramework)
                .Select(nearest => nearest.Link.Target().AsPackage()).OfType<PackageNode>();

            public IEnumerable<PackageNode> DependentPackages(string? targetFramework) => Inputs(PackageLabels.DependencyPackage)
                .GroupByTargetFramework().SelectNearest(targetFramework)
                .Select(nearest => nearest.Link.Source().AsPackage()).OfType<PackageNode>();


            public IEnumerable<AssemblyNode> RestoredAssemblies() => Outputs(AssemblyLabels.Restored)
                .Select(link => link.Target().AsAssembly()).OfType<AssemblyNode>();
        }

        public class PackageGrouping
        {
            private readonly HashSet<GraphLink> _links;
            private readonly HashSet<NuGetFramework> _frameworks;
            private readonly CompatibilityTable _table;

            public PackageGrouping(IEnumerable<GraphLink> links)
            {
                _links = links.ToHashSet();
                _frameworks = _links
                    .SelectMany(link => link.Labels)
                    .Select(label => label.ToString())
                    .Select(label => NuGetMoniker.Resolve(label)?.Framework)
                    .OfType<NuGetFramework>()
                    .ToHashSet();
                _table = new(_frameworks);
            }

            public IEnumerable<(GraphLink Link, NuGetFramework Framework)> SelectNearest(string? targetFramework)
            {
                NuGetFramework targetNugetFramework = NuGetMoniker.Resolve(targetFramework)?.Framework ?? NuGetFramework.AnyFramework;
                NuGetFramework? nearestNugetFramework = !_frameworks.Contains(NuGetFramework.AnyFramework)
                    ? _table.GetNearest(targetNugetFramework).FirstOrDefault()
                    : NuGetFramework.AnyFramework;
                return nearestNugetFramework.ToOptional()
                    .FlatMap(framework => framework.ToMoniker().ToOptional().Map(moniker => (framework, moniker)))
                    .SelectMany(nearest =>
                    {
                        (NuGetFramework framework, string moniker) = nearest;
                        return _links
                            .Where(link => link.Labels.ContainsAny(moniker))
                            .Select(link => (link, framework));
                    });
            }
        }

        public static class PackageLabels
        {
            public const string Package = nameof(Package);
            public const string AssemblyAsset = $"{Package}.{nameof(AssemblyAsset)}";
            public const string AssemblyReference = $"{Package}.{nameof(AssemblyReference)}";
            public const string PackageReference = $"{Package}.{nameof(PackageReference)}";
            public const string DependencyPackage = $"{Package}.{nameof(DependencyPackage)}";
        }
    }
}
