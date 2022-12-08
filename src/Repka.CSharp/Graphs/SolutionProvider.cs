using Microsoft.Build.Construction;

namespace Repka.Graphs
{
    public class SolutionProvider : GraphProvider
    {
        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                foreach (var solutionFile in directory.EnumerateFiles("*.sln", SearchOption.AllDirectories))
                {
                    SolutionFile? solution = solutionFile.ToSolution();
                    if (solution is not null)
                    {
                        GraphKey solutionKey = new(solutionFile.FullName);
                        yield return new GraphNodeToken(solutionKey, CSharpLabels.Solution);

                        foreach (var project in solution.ProjectsInOrder)
                        {
                            GraphKey projectKey = new(project.AbsolutePath);
                            yield return new GraphLinkToken(solutionKey, projectKey, CSharpLabels.SolutionRef);
                        }
                    }
                }
            }
        }
    }
}
