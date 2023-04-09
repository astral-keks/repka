using Microsoft.CodeAnalysis;
using Repka.Collections;
using Repka.Diagnostics;
using System.Collections.Immutable;

namespace Repka.Workspaces
{
    internal static class WorkspaceReporting
    {
        public static IEnumerable<Report> Report(this ReportProvider? provider, Workspace workspace, WorkspaceInspector inspector = default)
        {
            if (provider is not null)
            {
                if (inspector.Equals(default))
                    inspector = WorkspaceInspector.Default;
                using (ReportWriter writer = provider.GetWriter(inspector.Root, "diagnostics"))
                {
                    List<IGrouping<string, Project>> projectGroups = workspace.CurrentSolution.Projects
                        .GroupBy(project => inspector.GetProjectGroup(project))
                        .ToList();
                    IEnumerable<Report?> reports = projectGroups
                        .OrderBy(projects => projects.Key)
                        .SelectMany(projects => projects.AsParallel(8)
                            .Select(project =>
                            {
                                Report? report = project.ToReport(inspector);
                                if (report?.Records.Any() == true)
                                    writer.Write(report);
                                return report;
                            }));
                    foreach (var report in reports)
                    {
                        yield return report ?? new();
                    }
                }
            }
        }

        private static Report? ToReport(this Project project, WorkspaceInspector inspector)
        {
            return inspector.IsRelevantProject(project)
                ? new Report()
                {
                    Title = project.Name,
                    Text = $"Project {project.Name} at {project.FilePath}",
                    Records = project.Documents
                    .AsParallel().WithDegreeOfParallelism(8)
                    .Select(document => document.ToReport(inspector))
                    .OfType<Report>()
                    .Where(report => report.Records.Any())
                    .ToList()
                }
                : default;
        }

        private static Report? ToReport(this Document document, WorkspaceInspector inspector)
        {
            SemanticModel semantic = document.GetSemantic();
            ImmutableArray<Diagnostic> diagnostics = semantic.GetDiagnostics();
            return inspector.IsRelevantDocument(document)
                ? new Report()
                {
                    Text = document.FilePath ?? document.Name,
                    Records = diagnostics
                        .Select(diagnostic => diagnostic.ToReport(inspector))
                        .OfType<Report>()
                        .ToList()
                }
                : default;
        }

        private static Report? ToReport(this Diagnostic diagnostic, WorkspaceInspector inspector)
        {
            return inspector.IsRelevantDiagnostic(diagnostic)
                ? new Report()
                {
                    Text = diagnostic.ToString(),
                }
                : default;
        }
    }
}
