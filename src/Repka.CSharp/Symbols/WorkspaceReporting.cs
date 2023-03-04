using Microsoft.Build.Construction;
using Repka.Reports;

namespace Repka.Symbols
{
    internal static class WorkspaceReporting
    {
        public static Report GetDiagnositcReport(this WorkspaceSemantic semantic)
        {
            return new Report
            {
                Title = semantic.Syntax.RelativePath,
                Records = semantic.Errors
                    .Select(diagnostic => diagnostic.ToString())
                    .ToList()
            };
        }

        public static Report GetResolvedReferencesReport(this IEnumerable<WorkspaceReference> references)
        {
            Report report = new();
            foreach (var reference in references.Where(reference => reference.Exists))
            {
                report.Records.Add($"{reference.Name}:{reference.Version} -> {reference.Target}");
            }
            return report;
        }

        public static Report GetUnresolvedReferencesReport(this IEnumerable<WorkspaceReference> references)
        {
            ProjectRootElement project = ProjectRootElement.Create();

            ProjectPropertyGroupElement propertyGroup = project.AddPropertyGroup();
            propertyGroup.AddProperty("TargetFramework", "net48");

            ProjectItemGroupElement itemGroup = project.AddItemGroup();
            foreach (var reference in references.Where(reference => !reference.Exists))
            {
                if (reference.Version is not null)
                    itemGroup.AddItem("PackageReference", reference.Name).AddMetadata("Version", reference.Version);
            }

            using StringWriter writer = new();
            project.Save(writer);

            return new Report
            {
                Records = new List<string> { writer.ToString() }
            };
        }
    }
}
