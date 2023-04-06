using Microsoft.Build.Construction;
using Repka.Solutions;
using static Repka.Graphs.SolutionDsl;

namespace Repka.Graphs
{
    public class SolutionProvider : GraphProvider
    {
        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                int i = 0;
                Progress.Start($"Solutions");
                foreach (var solutionFile in directory.EnumerateFiles("*.sln", SearchOption.AllDirectories))
                {
                    Progress.Notify($"Solutions: {++i}");
                    foreach (var token in GetSolutionTokens(solutionFile))
                        graph.Add(token);

                }
                Progress.Finish($"Solutions: {i}");
            }
        }

        private IEnumerable<GraphToken> GetSolutionTokens(FileInfo solutionFile)
        {
            SolutionFile? solution = solutionFile.ToSolution();
            if (solution is not null)
            {
                GraphKey solutionKey = new(solutionFile.FullName);
                yield return new GraphNodeToken(solutionKey, SolutionLabels.IsSolution);

                foreach (var project in solution.ProjectsInOrder)
                {
                    GraphKey projectKey = new(project.AbsolutePath);
                    yield return new GraphLinkToken(solutionKey, projectKey, SolutionLabels.ContainsProject);
                }
            }
        }
    }
}
