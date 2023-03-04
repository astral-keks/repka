using Microsoft.Build.Construction;
using Repka.Solutions;

namespace Repka.Graphs
{
    public class SolutionProvider : GraphProvider
    {
        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                int i = 0;
                Progress.Start($"Solutions");
                foreach (var solutionFile in directory.EnumerateFiles("*.sln", SearchOption.AllDirectories))
                {
                    Progress.Notify($"Solutions: {++i}");
                    SolutionFile? solution = solutionFile.ToSolution();
                    if (solution is not null)
                    {
                        GraphKey solutionKey = new(solutionFile.FullName);
                        yield return new GraphNodeToken(solutionKey, CSharpLabels.IsSolution);

                        foreach (var project in solution.ProjectsInOrder)
                        {
                            GraphKey projectKey = new(project.AbsolutePath);
                            yield return new GraphLinkToken(solutionKey, projectKey, CSharpLabels.ContainsProject);
                        }
                    }
                }
                Progress.Finish($"Solutions: {i}");
            }
        }
    }
}
