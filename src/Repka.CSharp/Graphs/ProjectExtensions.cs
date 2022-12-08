using Microsoft.Build.Construction;
using Repka.Optionals;

namespace Repka.Graphs
{
    internal static class ProjectExtensions
    {
        public static ProjectRootElement ToProject(this FileInfo file)
        {
            try
            {
                return ProjectRootElement.Open(file.FullName);
            }
            catch
            {
                return ProjectRootElement.Create(file.FullName);
            }
        }

        public static string? GetPackageId(this ProjectRootElement project)
        {
            string? packageId = project.Properties.FirstOrDefault(property => property.ElementName == "PackageId")?.Value;
            return packageId; // !string.IsNullOrWhiteSpace(packageId) ? packageId : project.GetAssemblyName();
        }

        public static bool IsExeOutputType(this ProjectRootElement project)
        {
            return project.GetOutputType()?.Contains("exe", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsDllOutputType(this ProjectRootElement project)
        {
            return string.Equals(project.GetOutputType() ?? "Library", "Library", StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetOutputType(this ProjectRootElement project)
        {
            return project.Properties.FirstOrDefault(property => property.Name == "OutputType")?.Value;
        }

        public static IEnumerable<string> GetDllAssemblyPaths(this ProjectRootElement project)
        {
            return project.GetAssemblyPaths().Where(path => path.EndsWith(".dll"));
        }

        public static IEnumerable<string> GetAssemblyPaths(this ProjectRootElement project)
        {
            string? assemblyName = project.GetAssemblyName();
            if (!string.IsNullOrWhiteSpace(assemblyName))
            {
                string outputType = project.GetOutputType() ?? "";
                string extension = outputType.Contains("exe", StringComparison.OrdinalIgnoreCase) ? ".exe" : ".dll";
                
                yield return Path.Combine(project.DirectoryPath, "bin", "Debug", $"{assemblyName}.{extension}");
                yield return Path.Combine(project.DirectoryPath, "bin", "Release", $"{assemblyName}.{extension}");
            }
        }

        public static string? GetAssemblyName(this ProjectRootElement project)
        {
            string? fileName = Path.GetFileNameWithoutExtension(project.FullPath);
            string? assemblyName = project.Properties.FirstOrDefault(property => property.ElementName == "AssemblyName")?.Value;
            return !string.IsNullOrWhiteSpace(assemblyName) ? assemblyName : fileName;
        }

        public static IEnumerable<PackageReference> GetPackageReferences(this ProjectRootElement project)
        {
            return project.Items
                .Where(item => item.ElementName == "PackageReference")
                .Select(item =>
                {
                    string id = item.Include;
                    string? version = item.Metadata.FirstOrDefault(metadata => metadata.ElementName == "Version")?.Value;
                    return new PackageReference(id, version);
                });
        }

        public static IEnumerable<ProjectReference> GetProjectReferences(this ProjectRootElement project)
        {
            return project.FullPath.ToOptional()
                .Map(path => Path.GetDirectoryName(project.FullPath))
                .OfType<string>()
                .SelectMany(directory => project.Items
                    .Where(item => item.ElementName == "ProjectReference")
                    .Select(item => new ProjectReference(item.Include, Path.GetFullPath(Path.Combine(directory, item.Include)))));
        }

        public static IEnumerable<LibraryReference> GetDllReferences(this ProjectRootElement project)
        {
            return project.FullPath.ToOptional()
                .Map(path => Path.GetDirectoryName(project.FullPath))
                .OfType<string>()
                .SelectMany(directory => project.Items
                    .Where(item => item.ElementName == "Reference")
                    .Select(item => item.Metadata.FirstOrDefault(metadata => metadata.Name == "HintPath")?.Value)
                    .OfType<string>()
                    .Select(hintPath => new LibraryReference(hintPath, Path.GetFullPath(Path.Combine(directory, hintPath)))));
        }
    }
}
