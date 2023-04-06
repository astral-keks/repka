using Microsoft.Build.Construction;
using Repka.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using static Repka.Graphs.AssemblyDsl;

namespace Repka.Projects
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

        public static bool IsPackageable(this ProjectRootElement project)
        {
            return !string.IsNullOrWhiteSpace(project.GetPackageId());
        }

        public static bool IsExecutableOutputType(this ProjectRootElement project)
        {
            return project.GetOutputType()?.Contains("exe", StringComparison.OrdinalIgnoreCase) == true;
        }

        public static bool IsLibraryOutputType(this ProjectRootElement project)
        {
            return string.Equals(project.GetOutputType() ?? "Library", "Library", StringComparison.OrdinalIgnoreCase);
        }

        public static string? GetOutputType(this ProjectRootElement project)
        {
            return project.Properties.FirstOrDefault(property => property.Name == "OutputType")?.Value;
        }

        public static IEnumerable<string> GetOutputPaths(this ProjectRootElement project)
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

        public static ISet<string> GetTargetFrameworks(this ProjectRootElement project)
        {
            HashSet<string> frameworks = new();

            string? targetFrameworkVersion = project.Properties.FirstOrDefault(property => property.ElementName == "TargetFrameworkVersion")?.Value;
            if (targetFrameworkVersion is not null)
                frameworks.Add($"net{targetFrameworkVersion.Replace("v", "").Replace(".", "")}");
            string? targetFramework = project.Properties.FirstOrDefault(property => property.ElementName == "TargetFramework")?.Value;
            if (targetFramework is not null)
                frameworks.Add(targetFramework);
            string? targetFrameworks = project.Properties.FirstOrDefault(property => property.ElementName == "TargetFrameworks")?.Value;
            if (targetFrameworks is not null)
            {
                foreach (var framework in targetFrameworks.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    frameworks.Add(framework);
            }

            return frameworks;
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
                    string id = !string.IsNullOrEmpty(item.Include) ? item.Include : item.Update;
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

        public static IEnumerable<LibraryReference> GetLibraryReferences(this ProjectRootElement project)
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

        public static IEnumerable<FrameworkReference> GetFrameworkReferences(this ProjectRootElement project)
        {
            List<FrameworkReference> frameworkAssemblies = new();
            ISet<string> targetFrameworks = project.GetTargetFrameworks();
            if (targetFrameworks.Any(framework => Regex.IsMatch(framework, "^net[0-9]+")))
                frameworkAssemblies.Add(FrameworkReference.Mscorlib);
            if (targetFrameworks.Any(framework => framework.Contains("netstandard")))
                frameworkAssemblies.Add(FrameworkReference.Netstandard);
            return project.Items
                .Where(item => item.ElementName == "Reference" && !item.Metadata.Any(metadata => metadata.Name == "HintPath"))
                .Select(item => new FrameworkReference(new string(item.Include.TakeWhile(ch => ch != ',').ToArray())))
                .Concat(frameworkAssemblies);
        }

        public static IEnumerable<DocumentReference> GetDocumentLinks(this ProjectRootElement project)
        {
            return project.FullPath.ToOptional()
                .Map(path => Path.GetDirectoryName(project.FullPath))
                .OfType<string>()
                .SelectMany(directory => project.Items
                    .Where(item => item.ElementName == "Compile" && item.Include.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    .Select(item => new DocumentReference(item.Include, Path.GetFullPath(Path.Combine(directory, item.Include)))));
        }
    }
}
