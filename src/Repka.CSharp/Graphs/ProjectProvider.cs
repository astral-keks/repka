using Microsoft.Build.Construction;

namespace Repka.Graphs
{
    public class ProjectProvider : GraphProvider
    {
        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                foreach (var projectFile in directory.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
                {
                    ProjectRootElement project = projectFile.ToProject();

                    GraphKey projectKey = new(projectFile.FullName);
                    GraphNodeToken projectNode = new(projectKey, CSharpLabels.Project);

                    if (project.IsExeOutputType())
                        projectNode.Labels.Add(CSharpLabels.ExecutableProject);
                    if (project.IsDllOutputType())
                        projectNode.Labels.Add(CSharpLabels.LibraryProject);

                    foreach (string dllPath in project.GetDllAssemblyPaths())
                    {
                        GraphKey dllKey = new(dllPath);
                        yield return new GraphLinkToken(projectKey, dllKey, CSharpLabels.LibraryProject);
                    }

                    string? packageId = project.GetPackageId();
                    if (!string.IsNullOrWhiteSpace(packageId))
                    {
                        GraphKey packageKey = new PackageKey(packageId);
                        yield return new GraphNodeToken(packageKey, CSharpLabels.Package);
                        yield return new GraphLinkToken(projectKey, packageKey);
                        projectNode.Labels.Add(CSharpLabels.PackageProject);
                    }

                    foreach (var reference in project.GetPackageReferences())
                    {
                        GraphKey packageIdReferenceKey = new PackageKey(reference.Id);
                        yield return new GraphLinkToken(projectKey, packageIdReferenceKey, CSharpLabels.PackageIdRef);

                        GraphKey packageVersionReferenceKey = new PackageKey(reference.Id, reference.Version);
                        yield return new GraphLinkToken(projectKey, packageVersionReferenceKey, CSharpLabels.PackageVersionRef);
                    }

                    foreach (var reference in project.GetProjectReferences())
                    {
                        GraphKey projectReferenceKey = new(reference.AbsolutePath);
                        yield return new GraphLinkToken(projectKey, projectReferenceKey, CSharpLabels.ProjectRef);
                    }

                    foreach (var reference in project.GetDllReferences())
                    {
                        GraphKey dllReferenceKey = new(reference.AbsolutePath);
                        yield return new GraphLinkToken(projectKey, dllReferenceKey, CSharpLabels.LibraryRef);
                    }

                    yield return projectNode;
                }
            }
        }

        public override IEnumerable<GraphAttribute> GetAttributes(GraphToken token, Graph graph)
        {
            if (graph.Element(token) is GraphNode node && node.Labels.Contains(CSharpLabels.Project))
            {
                FileInfo file = new(node.Key);
                yield return new GraphAttribute<ProjectRootElement>(token, () => file.ToProject());
            }
        }
    }
}
