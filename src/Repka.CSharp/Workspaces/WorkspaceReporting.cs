using Microsoft.CodeAnalysis;
using Repka.Diagnostics;
using System.Collections.Immutable;

namespace Repka.Workspaces
{
    internal static class WorkspaceReporting
    {
        public static IEnumerable<Project> Report(this ReportProvider? provider, Workspace workspace, WorkspaceInspector? inspector = default)
        {
            if (provider is not null && workspace.CurrentSolution.FilePath is not null)
            {
                using (ReportWriter diagrnosticsWriter = provider.GetWriter(workspace.CurrentSolution.FilePath, "diagnostics"))
                {
                    IEnumerable<Project> projects = workspace.CurrentSolution.Projects.OrderBy(project => project.FilePath)
                        //.AsParallel().AsOrdered().WithDegreeOfParallelism(8)
                        .Select(project =>
                        {
                            Report report = project.ToReport(inspector ?? WorkspaceInspector.Default);
                            if (report.Records.Any())
                                diagrnosticsWriter.Write(report);
                            return project;
                        });
                    foreach (var project in projects)
                        yield return project;
                }
            }
        }

        private static Report ToReport(this Project project, WorkspaceInspector? inspector = default) => new()
        {
            Text = $"Project {project.Name} at {project.FilePath}",
            Records = project.Documents
                .Select(document => document.ToReport(inspector))
                .Where(report => report.Records.Any())
                .ToList()
        };

        private static Report ToReport(this Document document, WorkspaceInspector? inspector = default)
        {
            SemanticModel semantic = document.GetSemantic();
            ImmutableArray<Diagnostic> diagnostics = semantic.GetDiagnostics();
            return new Report()
            {
                Text = document.FilePath ?? document.Name,
                Records = diagnostics
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
