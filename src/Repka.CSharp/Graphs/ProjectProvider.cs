using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Packaging;
using Repka.Projects;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class ProjectProvider : GraphProvider
    {
        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                List<FileInfo> projectFiles = directory.EnumerateFiles("*.csproj", SearchOption.AllDirectories).AsParallel(8).ToList();
                ProgressPercentage projectProgress = Progress.Percent("Collecting projects", projectFiles.Count);
                IEnumerable<GraphToken> projectTokens = projectFiles.AsParallel(8)
                    .Peek(projectProgress.Increment)
                    .SelectMany(projectFile => GetProjectTokens(projectFile))
                    .ToList();
                foreach (var token in projectTokens)
                    graph.Add(token);
                projectProgress.Complete();

            }
        }

        private IEnumerable<GraphToken> GetProjectTokens(FileInfo projectFile)
        {
            ProjectRootElement projectElement = projectFile.ToProject();

            GraphKey projectKey = new(projectFile.FullName);
            GraphNodeToken projectToken = new(projectKey, ProjectLabels.Project);
            if (projectElement.IsExecutableOutputType())
                projectToken.Mark(ProjectLabels.Executable);
            if (projectElement.IsLibraryOutputType())
                projectToken.Mark(ProjectLabels.Library);
            foreach (var targetFramework in projectElement.GetTargetFrameworks())
                projectToken.Mark(ProjectLabels.TargetFramework, targetFramework);
            string? assemblyName = projectElement.GetAssemblyName();
            if (!string.IsNullOrWhiteSpace(assemblyName))
                projectToken.Mark(ProjectLabels.AssemblyName, assemblyName);
            yield return projectToken;

            if (!string.IsNullOrWhiteSpace(projectElement.Sdk))
            {
                GraphKey sdkKey = new(projectElement.Sdk);
                yield return new GraphLinkToken(projectKey, sdkKey, ProjectLabels.Sdk);
            }

            string? packageId = projectElement.GetPackageId();
            if (!string.IsNullOrWhiteSpace(packageId))
            {
                NuGetDescriptor packageDescriptor = new(packageId, default(string));
                GraphKey packageKey = new(packageDescriptor.ToString());
                yield return new GraphLinkToken(projectToken.Key, packageKey, ProjectLabels.Package);
            }

            foreach (var packageReference in projectElement.GetPackageReferences())
            {
                NuGetDescriptor packageReferenceDescriptor = new(packageReference.Id, packageReference.Version);
                GraphKey packageReferenceKey = new(packageReferenceDescriptor.ToString());
                yield return new GraphLinkToken(projectKey, packageReferenceKey, ProjectLabels.PackageReference);
            }

            foreach (var projectReference in projectElement.GetProjectReferences())
            {
                GraphKey projectReferenceKey = new(projectReference.AbsolutePath);
                yield return new GraphLinkToken(projectKey, projectReferenceKey, ProjectLabels.ProjectReference);
            }

            foreach (var libraryReference in projectElement.GetLibraryReferences())
            {
                GraphKey libraryReferenceKey = new(libraryReference.AbsolutePath);
                yield return new GraphLinkToken(projectKey, libraryReferenceKey, ProjectLabels.LibraryReference);
            }

            foreach (var assemblyReference in projectElement.GetFrameworkAssemblyReferences())
            {
                GraphKey assemblyReferenceKey = new(assemblyReference.AssemblyName);
                yield return new GraphLinkToken(projectKey, assemblyReferenceKey, ProjectLabels.FrameworkAssemblyReference);
            }

            foreach (var documentReference in projectElement.GetDocumentReferences())
            {
                GraphKey documentReferenceKey = new(documentReference.AbsolutePath);
                yield return new GraphLinkToken(projectKey, documentReferenceKey, ProjectLabels.DocumentReference);
            }
        }

    }
}
