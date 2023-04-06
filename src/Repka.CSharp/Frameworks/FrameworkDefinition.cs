using Repka.Assemblies;
using System.Collections.Concurrent;
using System.Reflection;

namespace Repka.Frameworks
{
    public class FrameworkDefinition
    {
        private readonly string? _moniker;
        private readonly List<string> _roots;
        private readonly List<AssemblyFile> _assemblies;
        private readonly ConcurrentDictionary<string, AssemblyFile?> _cache;

        public FrameworkDefinition(string? moniker, List<string> roots, List<string> assemblies)
            : this(moniker, roots)
        {
            _assemblies.AddRange(assemblies.Select(a => ResolveAssembly(a)).OfType<AssemblyFile>());
        }
        private FrameworkDefinition(string? moniker, List<string> roots, List<AssemblyFile> assemblies)
            : this(moniker, roots)
        {
            _assemblies.AddRange(assemblies);
        }
        private FrameworkDefinition(string? moniker, List<string> roots)
        {
            _moniker = moniker;
            _roots = roots;
            _assemblies = new();
            _cache = new();
        }

        public string? Moniker => _moniker;

        public IReadOnlyCollection<AssemblyFile> Assemblies => _assemblies;
        
        public AssemblyFile? ResolveAssembly(string? assemblyName)
        {
            return assemblyName is not null 
                ? _cache.GetOrAdd(assemblyName, _ =>
                {
                    AssemblyFile? assemblyFile = default;

                    foreach (var root in _roots)
                    {
                        string assenblyFileName = $"{assemblyName}.dll";
                        string assemblyLocation = Path.Combine(root, assenblyFileName);
                        assemblyFile = new AssemblyFile(assemblyLocation);
                        if (assemblyFile.Exists)
                            break;
                        else
                            assemblyFile = default;
                    }

                    return assemblyFile;
                })
                : default;
        }

        public static FrameworkDefinition operator &(FrameworkDefinition left, FrameworkDefinition right) => left.CombineWith(right);
        public FrameworkDefinition CombineWith(FrameworkDefinition frameworkDefinition)
        {
            return new(_moniker ?? frameworkDefinition._moniker, 
                _roots.Concat(frameworkDefinition._roots).ToList(), 
                _assemblies.Concat(frameworkDefinition._assemblies).ToList());
        }
    }
}
