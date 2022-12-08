namespace Repka.Graphs
{
    public static class GraphTraversal
    {
        public static IEnumerable<(GraphKey Key, int Level)> SourcesByLevel(this Graph graph, IEnumerable<GraphKey> keys)
        {
            return keys.SelectMany(key => graph.Sources(key, level: 0));
        }

        private static IEnumerable<(GraphKey Key, int Level)> Sources(this Graph graph, GraphKey key, int level)
        {
            yield return (key, level);
            foreach (var link in graph.Links(key).Where(link => link.TargetKey == key))
            {
                GraphKey sourceKey = link.SourceKey;
                foreach (var referer in graph.Sources(sourceKey, level + 1))
                {
                    yield return referer;
                }
            }
        }

        public static IEnumerable<GraphNode> TraverseBreadth(this GraphNode node, long maxDepth = long.MaxValue)
        {
            return node.TraverseBreadth(maxDepth, new HashSet<GraphKey>());
        }

        private static IEnumerable<GraphNode> TraverseBreadth(this GraphNode node, long maxDepth, HashSet<GraphKey> keys)
        {
            if (keys.Add(node.Key))
                yield return node;

            foreach (var subnode in node.Neighbors())
            {
                if (keys.Add(subnode.Key))
                    yield return subnode;
            }

            if (--maxDepth != 0)
            {
                foreach (var subnode in node.Neighbors().SelectMany(to => to.TraverseBreadth(maxDepth, keys)))
                {
                    yield return subnode;
                }
            }
        }
    }
}
