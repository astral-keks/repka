using Microsoft.CodeAnalysis;

namespace Repka.Workspaces
{
    public struct WorkspaceInspector
    {
        public static WorkspaceInspector Default { get; } = new()
        {
            IncludeErrors = new() { "CS0234", "CS0246", "CS0103" },
            IncludeSeverities = new() { DiagnosticSeverity.Error }
        };

        public string Root { get; init; }
        public string? ProjectMask { get; init; }
        public string? DocumentMask { get; init; }
        public HashSet<string>? IncludeProjects { get; init; }
        public HashSet<string>? ExcludeProjects { get; init; }
        public HashSet<string>? ExcludeErrors { get; init; }
        public HashSet<string>? IncludeErrors { get; init; }
        public HashSet<DiagnosticSeverity>? ExcludeSeverities { get; init; }
        public HashSet<DiagnosticSeverity>? IncludeSeverities { get; init; }

        public string? StripRoot(string? text)
        {
            return text?.Replace(Root, "~");
        }

        public bool IsRelevantProject(Project project)
        {
            return (ProjectMask is null || project.FilePath?.Contains(ProjectMask) == true) && 
                IsMatch(project.Name, IncludeProjects, ExcludeProjects);
        }

        public bool IsRelevantDocument(Document document)
        {
            return (DocumentMask is null || document.FilePath?.Contains(DocumentMask) == true);
        }

        public bool IsRelevantDiagnostic(Diagnostic diagnostic)
        {
            return !diagnostic.IsSuppressed &&
                IsMatch(diagnostic.Severity, IncludeSeverities, ExcludeSeverities) &&
                IsMatch(diagnostic.Id, IncludeErrors, ExcludeErrors);
        }

        private bool IsMatch<T>(T value, ISet<T>? include, ISet<T>? exclude)
        {
            return (include is null || include.Contains(value)) &&
                (exclude is null || !exclude.Contains(value));
        }
    }
}
