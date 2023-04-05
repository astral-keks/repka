﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Repka.Assemblies;
using Repka.FileSystems;
using Repka.Graphs;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Workspaces
{
    internal class WorkspaceBuilder
    {
        public WorkspaceReferences References { get; } = new();

        public AdhocWorkspace Workspace { get; } = new();

        public Solution AddSolution(GraphKey root)
        {
            string path = FileSystemPaths.Aux(root, "all.sln");
            return Workspace.AddSolution(SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), path));
        }

        public Project AddProject(ProjectNode projectNode)
        {
            HashSet<ProjectNode> projectDependencies = projectNode.ProjectDependencies.Traverse().ToHashSet();
            HashSet<AssemblyFile> assemblies = projectNode.AssemblyDependencies.ToHashSet();

            List<ProjectReference> projectReferences = projectDependencies
                .Distinct()
                .Select(projectNode => new ProjectReference(projectNode.Id))
                .ToList();
            List<MetadataReference> metadataReferences = assemblies
                .Where(assemblyFile => assemblyFile.Exists)
                .SelectMany(assemblyFile => References.GetOrAdd(assemblyFile.Path, () => MetadataReference.CreateFromFile(assemblyFile.Path)))
                .ToList();

            List<DocumentInfo> documents = projectNode.Documents
                .Select(documentNode => AddDocument(documentNode, projectNode))
                .ToList();

            ProjectInfo projectInfo = ProjectInfo.Create(projectNode.Id, VersionStamp.Create(), projectNode.Name, projectNode.Name, LanguageNames.CSharp,
                filePath: projectNode.Path, documents: documents, metadataReferences: metadataReferences, projectReferences: projectReferences);
            Project project;
            lock (Workspace)
                project = Workspace.AddProject(projectInfo);

            return project;
        }

        private DocumentInfo AddDocument(DocumentNode documentNode, ProjectNode projectNode)
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
