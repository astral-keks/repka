using System.Collections.Concurrent;

namespace Repka.Assemblies
{
    public class AssemblyResolver
    {
        private readonly List<string> _roots;
        private readonly ConcurrentDictionary<AssemblyName, AssemblyMetadata?> _cache;

        public AssemblyResolver(params AssemblyResolver[] resolvers)
            : this(resolvers.SelectMany(resolver => resolver._roots).ToList())
        {
        }

        public AssemblyResolver(List<string> roots)
        {
            _roots = roots;
            _cache = new();
        }

        public AssemblyMetadata? FindAssembly(AssemblyName? assemblyName)
        {
            return assemblyName is not null
                ? _cache.GetOrAdd(assemblyName, _ =>
                {
                    AssemblyMetadata? assembly = default;

                    foreach (var root in _roots)
                    {
                        string assenblyDll = $"{assemblyName}.dll";
                        string assemblyLocation = Path.Combine(root, assenblyDll);
                        assembly = new AssemblyMetadata(assemblyLocation);
                        if (assembly.Exists)
                            break;
                        else
                            assembly = default;
                    }

                    return assembly;
                })
                : default;
        }

    }
}
