using Microsoft.CodeAnalysis;
using Repka.Diagnostics;

namespace Repka.Workspaces
{
    internal static class WorkspaceReporting
    {
        public static void Report(this ReportProvider? provider, Workspace workspace, string store, params string[] projects)
        {
            if (provider is not null)
            {
                using (ReportWriter diagrnosticsWriter = provider.GetWriter(store, "diagnostics"))
                {
                    diagrnosticsWriter.Write(workspace.ToReport(WorkspaceInspector.Default with
                    {
                        IncludeProjects = projects.ToHashSet()
                    }));
                }
            }
        }

        public static Report ToReport(this IEnumerable<WorkspaceReferences> references) => new()
        {
            Text = "All unresolved references",
            Records = references.Select(references => references.ToReport())
        };

        public static Report ToReport(this WorkspaceReferences references) => new()
        {
            Text = "Unresolved references:",
            Records = references.Unresolved.Select(reference => new Report { Text = reference })
        };

        public static Report ToReport(this Workspace workspace, WorkspaceInspector? inspector = default) => new()
        {
            Text = "Diagnostics:",
            Records = workspace.CurrentSolution.Projects
                .Where(project => inspector is null || inspector.Value.IsRelevantProject(project))
                .Select(project => project.ToReport(inspector))
        };

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
