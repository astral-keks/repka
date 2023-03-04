using System.Diagnostics.CodeAnalysis;

namespace Repka.Graphs
{
    public static class PackageDsl
    {
        public static IEnumerable<Package> Packages(this Graph graph) => 
            Enumerable
                .Concat(
                    graph.Nodes().Select(Package.Of).OfType<Package>(),
                    graph.Links().Select(Package.Of).OfType<Package>())
                .Distinct(new Package.Comparer());

        public class Package
        {
            private readonly PackageKey _key;
            private readonly GraphNode? _node;
            private readonly Graph _graph;

            public static Package? Of(GraphNode? node)
            {
                PackageKey? key = node?.Labels.Contains(CSharpLabels.IsPackage) == true 
                    ? PackageKey.Parse(node.Key)
                    : default;
                return key is not null && node is not null ? new(key, node, node.Graph) : default;
            }

            public static Package? Of(GraphLink? link)
            {
                PackageKey? key = link?.Labels.Contains(CSharpLabels.UsesPackageVersion) == true
                    ? PackageKey.Parse(link.TargetKey)
                    : default;
                return key is not null && link is not null ? new(key, null, link.Graph) : default;
            }

            private Package(PackageKey key, GraphNode? node, Graph graph)
            {
                _key = key;
                _node = node;
                _graph = graph;
            }

            public GraphKey Key => _key;

            public string Id => _key.Id;

            public string? Version => _key.Version;

            public bool IsExternal => _node is null;

            public GraphNode? Origin => _graph.Links(new GraphKey(Id), CSharpLabels.DefinesPackage).FirstOrDefault()?.Source();

            public IEnumerable<GraphKey> Referers => _graph.Links(_key, CSharpLabels.UsesPackageId, CSharpLabels.UsesPackageVersion)
                .Where(link => link.TargetKey == _key)
                .Select(link => link.SourceKey);

            public class Comparer : IEqualityComparer<Package>
            {
                public bool Equals(Package? x, Package? y)
                {
                    return string.Equals(x?.Id, y?.Id, StringComparison.OrdinalIgnoreCase);
                }

                public int GetHashCode([DisallowNull] Package obj)
                {
                    return HashCode.Combine(obj.Id);
                }
            }
        }

        public class PackageKey : GraphKey
        {
            public static PackageKey? Parse(string packageKey)
            {
                PackageKey? key = default;

                string[] parts = packageKey.Split(':');
                if (parts.Length == 1)
                    key = new(parts[0]);
                else if (parts.Length == 2)
                    key = new(parts[0], parts[1]);

                return key;
            }

            public PackageKey(string packageId)
                : base(packageId)
            {
                Id = packageId;
            }

            public PackageKey(string packageId, string? packageVersion)
                : base($"{packageId}:{packageVersion ?? "undefined"}")
            {
                Id = packageId;
                Version = packageVersion;
            }

            public string Id { get; }

            public string? Version { get; }
        }
    }
}
