using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Workspaces;
using static Repka.Graphs.WorkspaceDsl;
using static Repka.Graphs.ProjectDsl;
using static Repka.Graphs.DocumentDsl;
using AssemblyMetadata = Repka.Assemblies.AssemblyMetadata;
using Microsoft.CodeAnalysis.CSharp;

namespace Repka.Graphs
{
    public class WorkspaceProvider : GraphProvider
    {
        public ReportProvider? ReportProvider { get; init; }
        public WorkspaceInspector WorkspaceInspector { get; init; } = WorkspaceInspector.Default;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            AdhocWorkspace workspace = new();

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Creating workspace", projectNodes.Count);
            Inspection<ProjectNode, AssemblyMetadata> assemblyDescriptorInspection = new();
            Inspection<AssemblyMetadata, MetadataReference> metadataReferenceInspection = new();
            IEnumerable<ProjectInfo> projectsInfo = projectNodes                
                .Peek(projectProgress.Increment)
                .Select(projectNode => CreateProject(projectNode,
                    assemblyDescriptorInspection,
                    metadataReferenceInspection));
            foreach (var projectInfo in projectsInfo)
                workspace.AddProject(projectInfo);
            projectProgress.Complete();

            List<Document> documents = workspace.CurrentSolution.Projects.SelectMany(project => project.Documents).ToList();

            ProgressPercentage diagnosticsProgress = Progress.Percent("Collecting syntax and semantics", documents.Count);
            documents.Peek(diagnosticsProgress.Increment).ForAll(document =>
            {
                GraphAttribute syntaxAttribute = new(WorkspaceAttributes.Syntax, document.GetSyntax);
                graph.Document(document.FilePath)?.State.Set(syntaxAttribute);
                GraphAttribute semanticAttribute = new(WorkspaceAttributes.Semantic, document.GetSemantic); ;
                graph.Document(document.FilePath)?.State.Set(semanticAttribute);
            });
            diagnosticsProgress.Complete();

            if (ReportProvider is not null)
            {
                diagnosticsProgress = Progress.Percent("Reporting diagnostics", projectNodes.Count);
                ReportProvider.Report(workspace, WorkspaceInspector with { Root = key })
                    .ForAll(_ => diagnosticsProgress.Increment());
                diagnosticsProgress.Complete();
            }
        }

        private ProjectInfo CreateProject(ProjectNode projectNode,
            Inspection<ProjectNode, AssemblyMetadata> assemblyDescriptorInspection, 
            Inspection<AssemblyMetadata, MetadataReference> metadataReferenceInspection)
        {
            ICollection<ProjectReference> projectReferences = GetProjectReferences(projectNode).ToList();
            ICollection<AssemblyMetadata> assemblyDependencies = GetAssemblyDependencies(projectNode, assemblyDescriptorInspection);
            ICollection<MetadataReference> metadataReferences = assemblyDependencies
                .SelectMany(assembly => GetMetadataReferences(assembly, metadataReferenceInspection))
                .ToList();
            List<DocumentInfo> documents = projectNode.Documents().AsParallel()
                .Select(documentNode => CreateDocument(documentNode, projectNode))
                .ToList();
            return ProjectInfo.Create(projectNode.Id, VersionStamp.Create(), projectNode.Name, projectNode.AssemblyName, LanguageNames.CSharp,
                filePath: projectNode.Location, documents: documents, metadataReferences: metadataReferences, projectReferences: projectReferences);
        }

        private DocumentInfo CreateDocument(DocumentNode documentNode, ProjectNode projectNode)
        {
            DocumentId documentId = DocumentId.CreateNewId(projectNode.Id);
            using Stream documentStream = documentNode.File().OpenRead();
            TextAndVersion documentText = TextAndVersion.Create(SourceText.From(documentStream), VersionStamp.Create(), documentNode.Location);
            DocumentInfo documentInfo = DocumentInfo.Create(documentId, documentNode.Name,
                loader: TextLoader.From(documentText), filePath: documentNode.Location);
            return documentInfo;
        }

        private IEnumerable<ProjectReference> GetProjectReferences(ProjectNode projectNode)
        {
            foreach (var dependencyProject in projectNode.RestoredProjects())
            {
                yield return new ProjectReference(dependencyProject.Id);
            }
        }

        private ICollection<AssemblyMetadata> GetAssemblyDependencies(ProjectNode projectNode,
            Inspection<ProjectNode, AssemblyMetadata> assemblyDescriptorInspection)
        {
            return assemblyDescriptorInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<AssemblyMetadata> visitProject()
            {
                IEnumerable<AssemblyMetadata> assemblies = projectNode.RestoredAssemblies()
                    .Select(assemblyNode => assemblyNode.Metadata)
                    .Where(assembly => assembly.Exists)
                    .GroupBy(assembly => assembly.Name)
                    .Select(assemblyGroup => assemblyGroup.MaxBy(assembly => assembly.Version))
                    .OfType<AssemblyMetadata>();
                foreach (var assembly in assemblies)
                    yield return assembly;
            }
        }

        private ICollection<MetadataReference> GetMetadataReferences(AssemblyMetadata assembly,
            Inspection<AssemblyMetadata, MetadataReference> metadataReferenceInspection)
        {
            return metadataReferenceInspection.InspectOrGet(assembly, () => visitAssembly().ToList());
            IEnumerable<MetadataReference> visitAssembly()
            {
                if (assembly.Exists)
                    yield return MetadataReference.CreateFromFile(assembly.Location);
            }
        }
    }
}
