using Repka.Collections;
using Repka.Diagnostics;
using Repka.FileSystems;
using Repka.Paths;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class DocumentProvider : GraphProvider
    {
        public List<RelativePath> CodegenDirectories { get; init; } = new(0);

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                List<ProjectNode> projectNodes = graph.Projects().ToList();
                ProgressPercentage projectProgress = Progress.Percent("Collecting document projects", projectNodes.Count);
                Dictionary<AbsolutePath, List<ProjectNode>> projectsByDocuments = GetProjectsByDocuments(projectNodes.Peek(projectProgress.Increment));
                Dictionary<AbsolutePath, List<ProjectNode>> projectsByDirectories = GetProjectsByDirectories(projectNodes.Peek(projectProgress.Increment));;
                projectProgress.Complete();

                FileInfo[] documentFiles = directory.GetFiles("*.cs", SearchOption.AllDirectories);
                ProgressPercentage documentProgress = Progress.Percent("Collecting documents", documentFiles.Length);
                IEnumerable<GraphToken> tokens = documentFiles.AsParallel(8)
                    .Peek(documentProgress.Increment)
                    .SelectMany(documentFile => GetDocumentTokens(documentFile, projectsByDocuments, projectsByDirectories))
                    .ToList();
                foreach (var token in tokens)
                    graph.Add(token);
                documentProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetDocumentTokens(FileInfo documentFile,
            Dictionary<AbsolutePath, List<ProjectNode>> projectsByDocuments,
            Dictionary<AbsolutePath, List<ProjectNode>> projectsByDirectories)
        {
            bool isIgnored = documentFile.FullName.Contains(@"\obj\") && (
                documentFile.Name.EndsWith("AssemblyAttributes.cs") ||
                documentFile.Name.EndsWith("AssemblyInfo.cs"));
            if (!isIgnored)
            {
                GraphKey documentKey = new(documentFile.FullName);
                yield return new GraphNodeToken(documentKey, DocumentLabels.Document);

                foreach (var documentProjectNode in GetDocumentProjects(documentFile, projectsByDocuments, projectsByDirectories))
                    yield return new GraphLinkToken(documentProjectNode.Key, documentKey, DocumentLabels.Document);
            }
        }

        private Dictionary<AbsolutePath, List<ProjectNode>> GetProjectsByDocuments(IEnumerable<ProjectNode> projectNodes)
        {
            return projectNodes
                .SelectMany(projectNode => projectNode.DocumentReferences
                    .Select(documentReference => (Document: documentReference, Project: projectNode)))
                .GroupBy(record => record.Document, record => record.Project)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        private Dictionary<AbsolutePath, List<ProjectNode>> GetProjectsByDirectories(IEnumerable<ProjectNode> projectNodes)
        {
            return projectNodes
                .GroupBy(projectNode => projectNode.Directory)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        private IEnumerable<ProjectNode> GetDocumentProjects(FileInfo documentFile, 
            Dictionary<AbsolutePath, List<ProjectNode>> projectsByDocuments,
            Dictionary<AbsolutePath, List<ProjectNode>> projectsByDirectories)
        {
            HashSet<ProjectNode> documentProjects = new();

            AbsolutePath documentPath = new(documentFile.FullName);
            if (projectsByDocuments.TryGetValue(documentPath, out var projectsByDocument))
            {
                documentProjects.AddRange(projectsByDocument);
            }
            else
            {
                bool isGeneratedDocument = CodegenDirectories.Any(documentPath.Includes);
                string? projectDirectory = documentFile.ParentDirectories()
                    .Where(directory => directory.EnumerateFiles("*.csproj").Any())
                    .Select(directory => directory.FullName)
                    .FirstOrDefault();
                if (projectDirectory is not null && projectsByDirectories.TryGetValue(projectDirectory, out var projectsByDirectory))
                    documentProjects.AddRange(projectsByDirectory.Where(projectNode => projectNode.HasSdk || isGeneratedDocument));
            }

            return documentProjects;
        }
    }
}
