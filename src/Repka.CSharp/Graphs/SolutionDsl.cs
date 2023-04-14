using Repka.Paths;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public static class SolutionDsl
    {
        public static IEnumerable<SolutionNode> Solutions(this Graph graph) => graph.Nodes()
            .Select(node => node.AsSolution()).OfType<SolutionNode>();

        public static IEnumerable<SolutionNode> Solutions(this Graph graph, string name) => graph.Solutions()
            .Where(solution => string.Equals(solution.Name, name, StringComparison.OrdinalIgnoreCase));

        public static SolutionNode? Solution(this Graph graph, GraphKey key) => graph.Node(key).AsSolution();

        public static SolutionNode? AsSolution(this GraphNode? node) =>
            node?.Labels.Contains(SolutionLabels.Solution) == true ? new(node) : default;

        public class SolutionNode : GraphNode
        {
            internal SolutionNode(GraphNode node) : base(node) { }

            public AbsolutePath Location => new(Key);

            public string? Name => Path.GetFileNameWithoutExtension(Location);

            public IEnumerable<AbsolutePath> ProjectReferences() => Outputs(SolutionLabels.SolutionProject)
                .Select(link => link.TargetKey.AsAbsolutePath());

            public IEnumerable<ProjectNode> Projects() => Outputs(SolutionLabels.SolutionProject)
                .Select(link => link.Target().AsProject()).OfType<ProjectNode>();
        }

        public static class SolutionLabels
        {
            public const string Solution = nameof(Solution);
            public const string SolutionProject = nameof(SolutionProject);
        }
    }
}
