using Repka.Diagnostics;

namespace Repka.Graphs
{
    public static class GraphReporting
    {
        public static Report? Report(this GraphNode node, string key, string[] labels)
        {
            GraphTraversal<GraphNode, Report?> traversal = new();
            return node.Report(key, labels.Select(label => new GraphLabel(label)).ToArray(), traversal);
        }

        private static Report? Report(this GraphNode node, GraphKey key, GraphLabel[] labels, 
            GraphTraversal<GraphNode, Report?> traversal)
        {
            return traversal.Visit(node, () => 
            {
                Report? result = default;

                if (!node.Key.Contains(key))
                {
                    HashSet<GraphLink> links = labels.SelectMany(label => node.Outputs(label)).ToHashSet();
                    List<Report> records = links
                        .Select(link => link.Target()?.Report(key, labels, traversal))
                        .OfType<Report>()
                        .ToList();
                    if (records.Any())
                        result = new Report { Text = node.Key, Records = records };
                }
                else
                    result = new Report() { Text = node.Key };

                return result;
            });
        }

        public static Report Report(this GraphNode node, params GraphLabel[] labels)
        {
            GraphTraversal<GraphNode, Report> traversal = new();
            return node.Report(labels, traversal);
        }

        private static Report Report(this GraphNode node, GraphLabel[] labels, 
            GraphTraversal<GraphNode, Report> traversal)
        {
            return traversal.Visit(node, () => new Report()
            {
                Text = node.Key,
                Records = labels.SelectMany(label => node.Outputs(label))
                    .Distinct()
                    .Select(link => link.Target())
                    .OfType<GraphNode>()
                    .Select(target => target.Report(labels, traversal))
                    .ToList()
            }) ?? new();
        }
    }
}
