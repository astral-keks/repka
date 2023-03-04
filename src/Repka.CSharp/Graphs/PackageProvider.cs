using Microsoft.Build.Construction;
using Repka.Projects;
using static Repka.Graphs.PackageDsl;

namespace Repka.Graphs
{
    public class PackageProvider : GraphProvider
    {
        public override IEnumerable<GraphToken> GetTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                int i = 0;
                Progress.Start($"Packages: ");
                foreach (var projectFile in directory.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
                {
                    ProjectRootElement project = projectFile.ToProject();
                    GraphKey projectKey = new(projectFile.FullName);

                    string? packageId = project.GetPackageId();
                    if (!string.IsNullOrWhiteSpace(packageId))
                    {
                        Progress.Notify($"Packages: {++i}");

                        GraphKey packageKey = new PackageKey(packageId);
                        yield return new GraphNodeToken(packageKey, CSharpLabels.IsPackage);
                        yield return new GraphLinkToken(projectKey, packageKey, CSharpLabels.DefinesPackage);
                    }

                    foreach (var reference in project.GetPackageReferences())
                    {
                        GraphKey packageIdReferenceKey = new PackageKey(reference.Id);
                        yield return new GraphLinkToken(projectKey, packageIdReferenceKey, CSharpLabels.UsesPackageId);

                        GraphKey packageVersionReferenceKey = new PackageKey(reference.Id, reference.Version);
                        yield return new GraphLinkToken(projectKey, packageVersionReferenceKey, CSharpLabels.UsesPackageVersion);
                    }
                }
                Progress.Finish($"Packages: {i}");
            }
        }
    }
}
