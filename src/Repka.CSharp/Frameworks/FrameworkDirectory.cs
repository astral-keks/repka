using Repka.Assemblies;
using System.Collections.Concurrent;

namespace Repka.Frameworks
{
    public class FrameworkDirectory
    {
        private readonly List<DirectoryInfo> _roots;
        private readonly Lazy<List<AssemblyFile>> _assemblies;
        private readonly ConcurrentDictionary<string, AssemblyFile?> _cache;

        public FrameworkDirectory(List<string> roots, List<string> assemblies)
        {
            _roots = roots.Select(root => new DirectoryInfo(root)).ToList();
            _assemblies = new(() => ResolveAssemblies(assemblies), true);
            _cache = new ConcurrentDictionary<string, AssemblyFile?>();
        }

        public IReadOnlyCollection<AssemblyFile> Assemblies => _assemblies.Value;
        private List<AssemblyFile> ResolveAssemblies(List<string> assemblyNames)
        {
            return assemblyNames
                .Select(ResolveAssembly)
                .OfType<AssemblyFile>()
                .ToList();
        }

        public AssemblyFile? ResolveAssembly(string? assemblyName)
        {
            return assemblyName is not null 
                ? _cache.GetOrAdd(assemblyName, _ =>
                {
                    AssemblyFile? assemblyFile = default;

                    foreach (var root in _roots)
                    {
                        string assenblyFileName = $"{assemblyName}.dll";
                        string assemblyLocation = Path.Combine(root.FullName, assenblyFileName);
                        assemblyFile = new(assemblyLocation);
                        if (assemblyFile.Exists)
                            break;
                        else
                            assemblyFile = default;
                    }

                    return assemblyFile;
                })
                : default;
        }
    }
}
