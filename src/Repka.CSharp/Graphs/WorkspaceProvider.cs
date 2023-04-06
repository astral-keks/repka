using Microsoft.CodeAnalysis;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Workspaces;
using static Repka.Graphs.WorkspaceDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class WorkspaceProvider : GraphProvider
    {
        public ReportProvider? ReportProvider { get; init; }

        public override void AddTokens(GraphKey key, Graph graph)
        {
            WorkspaceBuilder workspaceBuilder = new();
            workspaceBuilder.AddSolution(key);

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Creating workspace", projectNodes.Count);
            projectNodes.AsParallel().Peek(projectProgress.Increment).ForAll(projectNode => workspaceBuilder.AddProject(projectNode));
            projectProgress.Complete();

            AdhocWorkspace workspace = workspaceBuilder.Workspace;
            List<Document> documents = workspace.CurrentSolution.Projects.SelectMany(project => project.Documents).ToList();

            ProgressPercentage diagnosticsProgress = Progress.Percent("Collecting syntax and semantics", documents.Count);
            documents.Peek(diagnosticsProgress.Increment).ForAll(document =>
            {
                graph.Add(new SemanticAttribute(document));
                graph.Add(new SyntaxAttribute(document));
            });
            diagnosticsProgress.Complete();

            if (ReportProvider is not null)
            {
                diagnosticsProgress = Progress.Percent("Reporting diagnostics", projectNodes.Count);
                ReportProvider.Report(workspace).ForAll(_ => diagnosticsProgress.Increment());
                diagnosticsProgress.Complete();
            }
        }
    }
}
