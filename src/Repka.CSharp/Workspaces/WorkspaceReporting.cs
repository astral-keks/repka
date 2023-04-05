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
                    foreach (var project in workspace.CurrentSolution.Projects)
                    {
                        diagrnosticsWriter.Write(project.ToReport(inspector ?? WorkspaceInspector.Default));
                        yield return project;
                    }
                    
                }
            }
        }

        public static Report ToReport(this Project project, WorkspaceInspector? inspector = default) => new()
        {
            Text = $"Project {project.Name} at {project.FilePath}",
            Records = project.Documents
                .Select(document => document.ToReport(inspector))
                .Where(report => report.Records.Any())
                .ToList()
        };

        private static Report ToReport(this Document document, WorkspaceInspector? inspector = default)
        {
            SemanticModel? semantic = document.GetSemanticModelAsync().GetAwaiter().GetResult();
            return new Report()
            {
                Text = document.FilePath ?? document.Name,
                Records = (semantic?.GetDiagnostics() ?? Enumerable.Empty<Diagnostic>())
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
