using Repka.Collections;
using Repka.Diagnostics;
using Repka.Files;
using static Repka.Graphs.DocumentDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class DocumentProvider : GraphProvider
    {
        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                List<FileInfo> documentFiles = directory.EnumerateFiles("*.cs", SearchOption.AllDirectories).AsParallel()
                    .Where(documentFile => !(documentFile.FullName.Contains(@"\obj\") && (
                        documentFile.Name.EndsWith("AssemblyAttributes.cs") ||
                        documentFile.Name.EndsWith("AssemblyInfo.cs"))))
                    .ToList();
                ProgressPercentage documentProgress = Progress.Percent("Collecting documents", documentFiles.Count);
                foreach (var token in GetDocumentTokens(documentFiles.Peek(documentProgress.Increment), graph.Projects()))
                    yield return token;
                documentProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetDocumentTokens(IEnumerable<FileInfo> documentFiles, IEnumerable<ProjectNode> projectNodes)
        {
            Dictionary<string, HashSet<ProjectNode>> projectsByPath = projectNodes 
                .SelectMany(projectNode => projectNode.DocumentReferences
                    .Select(documentReference => (Path: documentReference, Project: projectNode))
                    .Prepend((Path: projectNode.Directory, Project: projectNode)))
                .GroupBy(mapping => mapping.Path, mapping => mapping.Project)
                .ToDictionary(group => group.Key, group => group.ToHashSet());
            foreach (var documentFile in documentFiles)
            {
                GraphKey documentKey = new(documentFile.FullName);
                yield return new GraphNodeToken(documentKey, DocumentLabels.Document);

                string? ownerDirectory = documentFile.ParentDirectories().Select(directory => directory.FullName).Prepend(documentFile.FullName)
                    .FirstOrDefault(path => projectsByPath.ContainsKey(path));
                HashSet<ProjectNode> ownerProjects = ownerDirectory is not null ? projectsByPath[ownerDirectory] : new(0);
                foreach (var ownerProjectNode in ownerProjects)
                {
                    yield return new GraphLinkToken(ownerProjectNode.Key, documentKey, DocumentLabels.Document);
                }
            }
        }
    }
}
