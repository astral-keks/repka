using Microsoft.Build.Construction;
using Repka.Diagnostics;
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
                FileInfo[] solutionFiles = directory.GetFiles("*.sln", SearchOption.AllDirectories);
                ProgressPercentage solutionProgress = Progress.Percent("Collecting solutions", solutionFiles.Length);
                foreach (var solutionFile in solutionFiles)
                {
                    solutionProgress.Increment();
                    foreach (var token in GetSolutionTokens(solutionFile))
                        graph.Add(token);

                }
                solutionProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetSolutionTokens(FileInfo solutionFile)
        {
            SolutionFile? solution = solutionFile.ToSolution();
            if (solution is not null)
            {
                GraphKey solutionKey = new(solutionFile.FullName);
                yield return new GraphNodeToken(solutionKey, SolutionLabels.Solution);

                foreach (var project in solution.ProjectsInOrder)
                {
                    GraphKey projectKey = new(project.AbsolutePath);
                    yield return new GraphLinkToken(solutionKey, projectKey, SolutionLabels.SolutionProject);
                }
            }
        }
    }
}
