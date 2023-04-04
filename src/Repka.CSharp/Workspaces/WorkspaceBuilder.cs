using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Repka.Assemblies;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Workspaces
{
    internal class WorkspaceBuilder
    {
        public WorkspaceReferences References { get; } = new();

        public AdhocWorkspace Workspace { get; } = new();

        public Project AddProject(ProjectNode projectNode)
        {
            (List<ProjectNode> projectDependencies, HashSet<AssemblyFile> projectAssemblies) = CreateProjectDependencies(projectNode);
            (List<PackageNode> packageDependencies, HashSet<AssemblyFile> packageAssemblies) = CreatePackageDependencies(projectNode, projectDependencies);

            List<ProjectReference> projectReferences = projectDependencies
                .Distinct()
                .Select(projectNode => new ProjectReference(projectNode.Id))
                .ToList();
            List<MetadataReference> metadataReferences = projectAssemblies.Union(packageAssemblies)
                .Where(assemblyFile => assemblyFile.Exists)
                .SelectMany(assemblyFile => References.GetOrAdd(assemblyFile.Path, () => MetadataReference.CreateFromFile(assemblyFile.Path)))
                .ToList();

            List<DocumentInfo> documents = projectNode.Documents
                .Select(documentNode => CreateDocument(documentNode, projectNode))
                .ToList();

            ProjectInfo projectInfo = ProjectInfo.Create(projectNode.Id, VersionStamp.Create(), projectNode.Name, projectNode.Name, LanguageNames.CSharp,
                filePath: projectNode.Path, documents: documents, metadataReferences: metadataReferences, projectReferences: projectReferences);
            Project project;
            lock (Workspace)
                project = Workspace.AddProject(projectInfo);

            return project;
        }

        private (List<ProjectNode>, HashSet<AssemblyFile>) CreateProjectDependencies(ProjectNode projectNode)
        {
            List<ProjectNode> projectDependencies = projectNode.ProjectDependencies.Traverse().ToList();
            HashSet<AssemblyFile> projectAssemblies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => Enumerable.Concat(
                    projectNode.FrameworkDependencies,
                    projectNode.LibraryDependencies))
                .ToHashSet();

            return (projectDependencies, projectAssemblies);
        }

        private (List<PackageNode>, HashSet<AssemblyFile>) CreatePackageDependencies(ProjectNode projectNode, List<ProjectNode> projectDependencies)
        {
            string? targetFramework = projectNode.TargetFramework;

            List<PackageNode> packageDependencies = projectDependencies.Prepend(projectNode)
                .SelectMany(projectNode => projectNode.PackageDependencies(targetFramework).Traverse())
                .Distinct()
                .GroupBy(packageNode => packageNode.Id)
                .Select(packageGroup => packageGroup.MaxBy(packageNode => packageNode.Version))
                .OfType<PackageNode>()
                .ToList();
            HashSet<AssemblyFile> packageAssemblies = packageDependencies
                .SelectMany(packageNode => Enumerable.Concat(
                    packageNode.PackageAssemblies(targetFramework),
                    packageNode.FrameworkDependencies(targetFramework)))
                .ToHashSet();

            return (packageDependencies, packageAssemblies);
        }

        private DocumentInfo CreateDocument(DocumentNode documentNode, ProjectNode projectNode)
        {
            DocumentId documentId = DocumentId.CreateNewId(projectNode.Id);
            using Stream documentStream = documentNode.Read();
            TextAndVersion documentText = TextAndVersion.Create(SourceText.From(documentStream), VersionStamp.Create(), documentNode.Path);
            DocumentInfo documentInfo = DocumentInfo.Create(documentId, documentNode.Name,
                loader: TextLoader.From(documentText), filePath: documentNode.Path);
            return documentInfo;
        }
    }
}
