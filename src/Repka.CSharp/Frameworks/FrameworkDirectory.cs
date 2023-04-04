using Repka.Assemblies;
using System.Collections.Concurrent;

namespace Repka.Frameworks
{
    public class FrameworkDirectory
    {
        private readonly DirectoryInfo _root;
        private readonly Lazy<List<AssemblyFile>> _assemblies;
        private readonly ConcurrentDictionary<string, AssemblyFile?> _cache;

        public FrameworkDirectory(string root, List<string> assemblies)
        {
            _root = new DirectoryInfo(root);
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
                    string assenblyFileName = $"{assemblyName}.dll";
                    string assemblyLocation = Path.Combine(_root.FullName, assenblyFileName);
                    AssemblyFile assemblyFile = new(assemblyLocation);
                    return assemblyFile.Exists ? assemblyFile : default;
                })
                : default;
        }
    }
}
