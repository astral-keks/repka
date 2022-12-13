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
                int i = 0;
                Progress.Start($"Projects and packages");
                foreach (var projectFile in directory.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
                {
                    Progress.Report($"Projects and packages: {++i}");
                    ProjectRootElement project = projectFile.ToProject();

                    GraphKey projectKey = new(projectFile.FullName);
                    GraphNodeToken projectNode = new(projectKey, CSharpLabels.IsProject);

                    if (project.IsExeOutputType())
                        projectNode.Labels.Add(CSharpLabels.DefinesExecutable);
                    if (project.IsDllOutputType())
                        projectNode.Labels.Add(CSharpLabels.DefinesLibrary);

                    foreach (string dllPath in project.GetDllAssemblyPaths())
                    {
                        GraphKey dllKey = new(dllPath);
                        yield return new GraphLinkToken(projectKey, dllKey, CSharpLabels.DefinesLibrary);
                    }

                    string? packageId = project.GetPackageId();
                    if (!string.IsNullOrWhiteSpace(packageId))
                    {
                        GraphKey packageKey = new PackageKey(packageId);
                        yield return new GraphNodeToken(packageKey, CSharpLabels.IsPackage);
                        yield return new GraphLinkToken(projectKey, packageKey);
                        projectNode.Labels.Add(CSharpLabels.DefinesPackage);
                    }

                    foreach (var reference in project.GetPackageReferences())
                    {
                        GraphKey packageIdReferenceKey = new PackageKey(reference.Id);
                        yield return new GraphLinkToken(projectKey, packageIdReferenceKey, CSharpLabels.UsesPackageId);

                        GraphKey packageVersionReferenceKey = new PackageKey(reference.Id, reference.Version);
                        yield return new GraphLinkToken(projectKey, packageVersionReferenceKey, CSharpLabels.UsesPackageVersion);
                    }

                    foreach (var reference in project.GetProjectReferences())
                    {
                        GraphKey projectReferenceKey = new(reference.AbsolutePath);
                        yield return new GraphLinkToken(projectKey, projectReferenceKey, CSharpLabels.UsesProject);
                    }

                    foreach (var reference in project.GetDllReferences())
                    {
                        GraphKey dllReferenceKey = new(reference.AbsolutePath);
                        yield return new GraphLinkToken(projectKey, dllReferenceKey, CSharpLabels.UsesLibrary);
                    }

                    yield return projectNode;
                }
                Progress.Finish($"Projects and packages: {i}");
            }
        }
    }
}
