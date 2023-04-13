﻿using Repka.Assemblies;
using Repka.Paths;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class AssemblyDsl
    {
        public static IEnumerable<AssemblyNode> Assemblies(this Graph graph) => graph.Nodes()
            .Select(node => node.AsAssembly()).OfType<AssemblyNode>();

        public static AssemblyNode? Assembly(this Graph graph, GraphKey key) => graph.Node(key).AsAssembly();

        public static AssemblyNode? AsAssembly(this GraphNode? node) => node?.Labels.Contains(AssemblyLabels.Assembly) == true
            ? new AssemblyNode(node)
            : default;

        public class AssemblyNode : GraphNode
        {
            internal AssemblyNode(GraphNode node) : base(node) { }

            public AbsolutePath Location => new(Key);

            public string? Name => Metadata.Name;

            public Version? Version => Metadata.Version;

            public AssemblyMetadata Metadata => Attribute(AssemblyAttributes.Metadata)
                .Value(() => new AssemblyMetadata(Location));

            public PackageNode? Package => Inputs(AssemblyLabels.Assembly)
                .Select(link => link.Source().AsPackage()).OfType<PackageNode>()
                .SingleOrDefault();

            public IEnumerable<ProjectNode> DependentProjects => Inputs(AssemblyLabels.Restored)
                .Select(link => link.Source()).OfType<ProjectNode>();

            public IEnumerable<PackageNode> DependentPackages => Inputs(AssemblyLabels.Restored)
                .Select(link => link.Source()).OfType<PackageNode>();
        }

        public static class AssemblyLabels
        {
            public const string Assembly = nameof(Assembly);
            public const string Restored = $"{Assembly}.{nameof(Restored)}";
        }

        public static class AssemblyAttributes
        {
            public const string Metadata = nameof(Metadata);
        }
    }
}
