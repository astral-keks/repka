using Repka.Collections;
using Repka.Diagnostics;
using Repka.FileSystems;
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
                    .Where(documentFile => !documentFile.FullName.Contains(@"\obj\"))
                    //.Where(documentFile => !(documentFile.FullName.Contains(@"\obj\") && (
                    //    documentFile.Name.EndsWith("AssemblyAttributes.cs") ||
                    //    documentFile.Name.EndsWith("AssemblyInfo.cs"))))
                    .ToList();
                ProgressPercentage documentProgress = Progress.Percent("Collecting documents", documentFiles.Count);
                foreach (var token in GetDocumentTokens(documentFiles.Peek(documentProgress.Increment), graph.Projects()))
                    yield return token;
                documentProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetDocumentTokens(IEnumerable<FileInfo> documentFiles, IEnumerable<ProjectNode> projectNodes)
        {
            Dictionary<string, List<ProjectNode>> projectsByPath = projectNodes 
                .GroupBy(projectNode => projectNode.Directory)
                .ToDictionary(group => group.Key, group => group.ToList());
            foreach (var documentFile in documentFiles)
            {
                GraphKey documentKey = new(documentFile.FullName);
                yield return new GraphNodeToken(documentKey, DocumentLabels.Document);

                string? ownerDirectory = documentFile.ParentDirectories().Select(directory => directory.FullName)
                    .FirstOrDefault(path => projectsByPath.ContainsKey(path));
                if (ownerDirectory is not null)
                {
                    IEnumerable<ProjectNode> ownerProjects = projectsByPath[ownerDirectory]
                        .Where(ownerProject => ownerProject.HasSdk || ownerProject.DocumentReferences.Contains(documentFile.FullName));
                    foreach (var ownerProjectNode in ownerProjects)
                    {
                        yield return new GraphLinkToken(ownerProjectNode.Key, documentKey, DocumentLabels.Document);
                    }
                }
            }
        }
    }
}
