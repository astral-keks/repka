using Microsoft.CodeAnalysis;
using Repka.Diagnostics;
using Repka.FileSystems;

namespace Repka.Workspaces
{
    internal static class WorkspaceReporting
    {
        public static IEnumerable<Project> Report(this ReportProvider? provider, Workspace workspace, WorkspaceInspector? inspector = default)
        {
            if (provider is not null && workspace.CurrentSolution.FilePath is not null)
            {
                string aux = FileSystemPaths.Aux(workspace.CurrentSolution.FilePath);
                using (ReportWriter diagrnosticsWriter = provider.GetWriter(aux, "diagnostics"))
                {
                    IEnumerable<(Project, Report)> reports = workspace.CurrentSolution.Projects
                        .OrderBy(project => project.FilePath)
                        .AsParallel().WithDegreeOfParallelism(8)
                        .Select(project => (project, project.ToReport(inspector ?? WorkspaceInspector.Default)));
                    foreach (var (project, report) in reports)
                    {
                        if (report.Records.Any())
                            diagrnosticsWriter.Write(report);
                        yield return project;
                    }
                }
            }
        }

        public static Report ToReport(this Project project, WorkspaceInspector? inspector = default) => new()
        {
            Text = $"Project {project.Name} at {project.FilePath}",
            Records = project.Documents.OrderBy(document => document.FilePath)
                .Select(document => document.ToReport(inspector))
                .Where(report => report.Records.Any())
                .ToList()
        };

        private static Report ToReport(this Document document, WorkspaceInspector? inspector = default)
        {
            SemanticModel semantic = document.GetSemantic();
            return new Report()
            {
                Text = document.FilePath ?? document.Name,
                Records = semantic.GetDiagnostics()
                    .Where(diagnostic => inspector is null || inspector.Value.IsRelevantDiagnostic(diagnostic))
                    .Select(diagnostic => diagnostic.ToReport())
                    .ToList()
            };
        }

        private static Report ToReport(this Diagnostic diagnostic) => new()
        {
            Text = diagnostic.ToString(),
        };
    }
}
