﻿using Microsoft.CodeAnalysis;

namespace Repka.Workspaces
{
    public struct WorkspaceInspector
    {
        public static WorkspaceInspector Default { get; } = new()
        {
            //ExcludeErrors = new() { "CS0433" },
            IncludeSeverities = new() { DiagnosticSeverity.Error }
        };

        public HashSet<string>? IncludeProjects { get; init; }
        public HashSet<string>? ExcludeProjects { get; init; }
        public HashSet<string>? ExcludeErrors { get; init; }
        public HashSet<string>? IncludeErrors { get; init; }
        public HashSet<DiagnosticSeverity>? ExcludeSeverities { get; init; }
        public HashSet<DiagnosticSeverity>? IncludeSeverities { get; init; }

        public bool IsRelevantProject(Project project)
        {
            return IsMatch(project.Name, IncludeProjects, ExcludeProjects);
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
