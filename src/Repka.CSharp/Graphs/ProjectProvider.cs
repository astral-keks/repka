using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis;
using Repka.Assemblies;
using Repka.Collections;
using Repka.Diagnostics;
using Repka.Frameworks;
using Repka.Packaging;
using Repka.Projects;
using static Repka.Graphs.PackageDsl;
using static Repka.Graphs.ProjectDsl;

namespace Repka.Graphs
{
    public class ProjectProvider : GraphProvider
    {
        public FrameworkDefinition Framework { get; init; } = FrameworkDefinitions.Current;

        public override void AddTokens(GraphKey key, Graph graph)
        {
            DirectoryInfo directory = new(key);
            if (directory.Exists)
            {
                List<FileInfo> projectFiles = directory.EnumerateFiles("*.csproj", SearchOption.AllDirectories).ToList();
                ProgressPercentage projectProgress = Progress.Percent("Collecting projects", projectFiles.Count);
                IEnumerable<GraphToken> tokens = projectFiles.AsParallel().WithDegreeOfParallelism(8)
                    .Peek(projectProgress.Increment)
                    .SelectMany(projectFile => GetProjectTokens(projectFile));
                foreach (var token in tokens)
                    graph.Add(token);
                projectProgress.Complete();

                List<ProjectNode> projectNodes = graph.Projects().ToList();
                ProgressPercentage dependencyProgress = Progress.Percent("Collecting project dependencies", projectNodes.Count);
                foreach (var token in GetDependencyTokens(projectNodes.Peek(dependencyProgress.Increment)))
                    graph.Add(token);
                dependencyProgress.Complete();
            }
        }

        private IEnumerable<GraphToken> GetProjectTokens(FileInfo projectFile)
        {
            ProjectRootElement projectElement = projectFile.ToProject();

            GraphKey projectKey = new(projectFile.FullName);
            GraphNodeToken projectToken = new(projectKey, ProjectLabels.Project);
            if (projectElement.IsExecutableOutputType())
                projectToken.Label(ProjectLabels.ExecutableProject);
            if (projectElement.IsLibraryOutputType())
                projectToken.Label(ProjectLabels.LibraryProject);
            if (projectElement.IsPackageable())
                projectToken.Label(ProjectLabels.PackageProject);
            yield return projectToken;

            if (!string.IsNullOrWhiteSpace(projectElement.Sdk))
            {
                GraphKey sdkKey = new(projectElement.Sdk);
                yield return new GraphLinkToken(projectKey, sdkKey, ProjectLabels.Sdk);
            }

            string? packageId = projectElement.GetPackageId();
            if (!string.IsNullOrWhiteSpace(packageId))
            {
                GraphKey packageKey = new(packageId);
                yield return new GraphLinkToken(projectToken.Key, packageKey, ProjectLabels.PackageDefinition);
            }

            foreach (var framework in projectElement.GetTargetFrameworks())
            {
                GraphKey frameworkKey = new(framework);
                yield return new GraphLinkToken(projectKey, frameworkKey, ProjectLabels.TargetFramework);
            }
        
            foreach (var packageReference in projectElement.GetPackageReferences())
            {
                PackageKey packageReferenceKey = new(packageReference.Id, packageReference.Version);
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

            foreach (var frameworkReference in projectElement.GetFrameworkReferences())
            {
                GraphKey frameworkReferenceKey = new(frameworkReference.AssemblyName);
                yield return new GraphLinkToken(projectKey, frameworkReferenceKey, ProjectLabels.FrameworkReference);

                IEnumerable<AssemblyDescriptor> frameworkAssemblies = Framework.Assemblies
                    .Append(Framework.Resolver.FindAssembly(frameworkReference.AssemblyName))
                    .OfType<AssemblyDescriptor>();
                foreach (var frameworkAssembly in frameworkAssemblies)
                {
                    GraphKey frameworkAssemblyKey = new(frameworkAssembly.Location);
                    yield return new GraphLinkToken(projectKey, frameworkAssemblyKey, ProjectLabels.FrameworkDependency);
                }
            }

            foreach (var documentReference in projectElement.GetDocumentReferences())
            {
                GraphKey documentReferenceKey = new(documentReference.AbsolutePath);
                yield return new GraphLinkToken(projectKey, documentReferenceKey, ProjectLabels.DocumentReference);
            }
        }

        private IEnumerable<GraphToken> GetDependencyTokens(IEnumerable<ProjectNode> projectNodes)
        {
            Dictionary<NuGetIdentifier, ProjectNode> packagableNodes = projectNodes
                .Where(projectNode => projectNode.PackageId is not null)
                .ToDictionary(projectNode => projectNode.PackageId!);

            foreach (var projectNode in projectNodes)
            {
                foreach (var dependencyNode in GetProjectDependencies(projectNode, packagableNodes))
                    yield return new GraphLinkToken(projectNode.Key, dependencyNode.Key, ProjectLabels.ProjectDependency);
            }
        }

        private IEnumerable<ProjectNode> GetProjectDependencies(ProjectNode projectNode, Dictionary<NuGetIdentifier, ProjectNode> packagableNodes)
        {
            foreach (var referenceNode in projectNode.ProjectReferences().ToList())
                yield return referenceNode;

            foreach (var referenceDescriptor in projectNode.PackageReferences.ToList())
            {
                if (packagableNodes.TryGetValue(referenceDescriptor.Id, out ProjectNode? packagableNode))
                    yield return packagableNode;
            }
        }
    }
}
