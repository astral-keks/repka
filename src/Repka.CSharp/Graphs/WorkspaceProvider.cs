using Microsoft.CodeAnalysis;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Workspaces;
using static Repka.Graphs.WorkspaceDsl;
using static Repka.Graphs.ProjectDsl;
using static Repka.Graphs.AssemblyDsl;
using Repka.Assemblies;
using Repka.FileSystems;
using Microsoft.CodeAnalysis.Text;
using static Repka.Graphs.DocumentDsl;

namespace Repka.Graphs
{
    public class WorkspaceProvider : GraphProvider
    {
        public ReportProvider? ReportProvider { get; init; }

        public override void AddTokens(GraphKey key, Graph graph)
        {
            AdhocWorkspace workspace = new();
            workspace.AddSolution(CreateSolution(key));

            List<ProjectNode> projectNodes = graph.Projects().ToList();
            ProgressPercentage projectProgress = Progress.Percent("Creating workspace", projectNodes.Count);
            Inspection<ProjectNode, ProjectReference> projectReferenceInspection = new();
            Inspection<ProjectNode, AssemblyDescriptor> assemblyDescriptorInspection = new();
            Inspection<AssemblyDescriptor, MetadataReference> metadataReferenceInspection = new();
            IEnumerable<ProjectInfo> projectsInfo = projectNodes.AsParallel(8)                
                .Peek(projectProgress.Increment)
                .Select(projectNode => CreateProject(projectNode,
                    projectReferenceInspection,
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
                GraphAttribute semanticAttribute = new(WorkspaceAttributes.Semantic, document.GetSemantic);
                graph.Document(document.FilePath)?.State.Set(semanticAttribute);
            });
            diagnosticsProgress.Complete();

            if (ReportProvider is not null)
            {
                diagnosticsProgress = Progress.Percent("Reporting diagnostics", projectNodes.Count);
                ReportProvider.Report(workspace).ForAll(_ => diagnosticsProgress.Increment());
                diagnosticsProgress.Complete();
            }
        }

        private SolutionInfo CreateSolution(GraphKey root)
        {
            string path = FileSystemPaths.Aux(root, "all.sln");
            return SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), path);
        }

        private ProjectInfo CreateProject(ProjectNode projectNode,
            Inspection<ProjectNode, ProjectReference> projectReferenceInspection,
            Inspection<ProjectNode, AssemblyDescriptor> assemblyDescriptorInspection,
            Inspection<AssemblyDescriptor, MetadataReference> metadataReferenceInspection)
        {
            ICollection<ProjectReference> projectReferences = GetProjectReferences(projectNode, projectReferenceInspection);
            ICollection<AssemblyDescriptor> assemblyDependencies = GetAssemblyDependencies(projectNode, assemblyDescriptorInspection);
            ICollection<MetadataReference> metadataReferences = assemblyDependencies
                .SelectMany(assembly => GetMetadataReferences(assembly, metadataReferenceInspection))
                .ToList();
            List<DocumentInfo> documents = projectNode.Documents.AsParallel()
                .Select(documentNode => CreateDocument(documentNode, projectNode))
                .ToList();
            return ProjectInfo.Create(projectNode.Id, VersionStamp.Create(), projectNode.Name, projectNode.Name, LanguageNames.CSharp,
                filePath: projectNode.Location, documents: documents, metadataReferences: metadataReferences, projectReferences: projectReferences);
        }

        private DocumentInfo CreateDocument(DocumentNode documentNode, ProjectNode projectNode)
        {
            DocumentId documentId = DocumentId.CreateNewId(projectNode.Id);
            using Stream documentStream = documentNode.Read();
            TextAndVersion documentText = TextAndVersion.Create(SourceText.From(documentStream), VersionStamp.Create(), documentNode.Location);
            DocumentInfo documentInfo = DocumentInfo.Create(documentId, documentNode.Name,
                loader: TextLoader.From(documentText), filePath: documentNode.Location);
            return documentInfo;
        }

        private ICollection<ProjectReference> GetProjectReferences(ProjectNode projectNode, 
            Inspection<ProjectNode, ProjectReference> projectReferenceInspection)
        {
            return projectReferenceInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<ProjectReference> visitProject()
            {
                foreach (var projectDependency in projectNode.ProjectDependencies)
                {
                    yield return new ProjectReference(projectDependency.Id);
                    foreach (var reference in GetProjectReferences(projectDependency, projectReferenceInspection))
                        yield return reference;
                }
            }
        }

        private ICollection<AssemblyDescriptor> GetAssemblyDependencies(ProjectNode projectNode,
            Inspection<ProjectNode, AssemblyDescriptor> assemblyDescriptorInspection)
        {
            return assemblyDescriptorInspection.InspectOrGet(projectNode, () => visitProject().ToHashSet());
            IEnumerable<AssemblyDescriptor> visitProject()
            {
                IEnumerable<AssemblyDescriptor> assemblies = projectNode.AssemblyDependencies()
                    .Select(assemblyNode => assemblyNode.Descriptor)
                    .Where(assembly => assembly.Exists)
                    .GroupBy(assembly => assembly.Name)
                    .Select(assemblyGroup => assemblyGroup.MaxBy(assembly => assembly.Version))
                    .OfType<AssemblyDescriptor>();
                foreach (var assembly in assemblies)
                    yield return assembly;
            }
        }

        private ICollection<MetadataReference> GetMetadataReferences(AssemblyDescriptor assembly,
            Inspection<AssemblyDescriptor, MetadataReference> metadataReferenceInspection)
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
