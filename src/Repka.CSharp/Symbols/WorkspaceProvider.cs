using Microsoft.CodeAnalysis;
using Repka.Diagnostics;
using Repka.Gac;
using Repka.Packages;
using Repka.Projects;
using static Repka.Diagnostics.Progress;

namespace Repka.Symbols
{
    public class WorkspaceProvider
    {
        public NuGetProvider NuGetProvider { get; init; } = new();

        public GacProvider GacProvider { get; init; } = new();

        public Progress Progress { get; init; } = new StdIO();

        public int Threads { get; init; } = 1;

        public Workspace CreateWorkspace(string root, IEnumerable<WorkspaceReference> references)
        {
            List<WorkspaceSyntax> syntaxes = new();

            DirectoryInfo directory = new(root);
            if (directory.Exists)
            {
                FileInfo[] sourceFiles = directory.GetFiles("*.cs", SearchOption.AllDirectories);
                syntaxes.AddRange(sourceFiles.WithProgress(Progress, "Source files")
                    .AsParallel().WithDegreeOfParallelism(Threads)
                    .Where(file => !file.FullName.Contains(@"\bin\") && !file.FullName.Contains(@"\obj\"))
                    .Select(file => new WorkspaceSyntax(root, file)));
            }

            return new Workspace(syntaxes, references);
        }

        public ICollection<WorkspaceReference> GetWorkspaceReferences(string root)
        {
            HashSet<WorkspaceReference> references = new();
            HashSet<string> projects = new();

            DirectoryInfo directory = new(root);
            if (directory.Exists)
            {
                GacDirectory gacDirectory = GacProvider.GetGacDirectory();
                NuGetDirectory nugetDirectory = NuGetProvider.GetGlobalPackagesDirectory(root);

                FileInfo[] projectFiles = directory.GetFiles("*.csproj", SearchOption.AllDirectories);
                projectFiles.WithProgress(Progress, "Project files")
                    .AsParallel().WithDegreeOfParallelism(Threads)
                    .Select(projectFile => projectFile.ToProject())
                    .ForAll(project =>
                    {
                        string? projectPackageId = project.GetPackageId();
                        if (projectPackageId is not null)
                            projects.Add(projectPackageId);

                        List<WorkspaceReference> gacReferences = project.GetGacReferences()
                            .SelectMany(gacRef => gacDirectory.ResolveAssembly(gacRef.Name)
                                .Select(assembly => new WorkspaceReference(gacRef.Name, assembly.FullName)))
                            .ToList();
                        List<WorkspaceReference> dllReferences = project.GetDllReferences()
                            .Select(libraryRef => new WorkspaceReference(libraryRef.RelativePath, libraryRef.AbsolutePath))
                            .ToList();
                        List<WorkspaceReference> packageReferences = project.GetPackageReferences()
                            .SelectMany(packageRef => nugetDirectory.ResolvePackage(packageRef.Id, packageRef.Version)
                                .Select(packageLib => new WorkspaceReference(packageRef.Id, packageRef.Version, packageLib.FullName)))
                            .ToList();
                        lock (references)
                        {
                            foreach (var gacReference in gacReferences) 
                                references.Add(gacReference);
                            foreach (var dllReference in dllReferences)
                                references.Add(dllReference);
                            foreach (var packageReference in packageReferences)
                                references.Add(packageReference);
                        }
                    });
            }

            return references.Where(reference => !projects.Contains(reference.Name)).ToList();
        }

    }
}
