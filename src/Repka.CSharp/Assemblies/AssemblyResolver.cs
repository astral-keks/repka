using System.Collections.Concurrent;

namespace Repka.Assemblies
{
    public class AssemblyResolver
    {
        private readonly List<string> _roots;
        private readonly ConcurrentDictionary<string, AssemblyDescriptor?> _cache;

        public AssemblyResolver(params AssemblyResolver[] resolvers)
            : this(resolvers.SelectMany(resolver => resolver._roots).ToList())
        {
        }

        public AssemblyResolver(List<string> roots)
        {
            _roots = roots;
            _cache = new();
        }

        public AssemblyDescriptor? FindAssembly(string? assemblyName)
        {
            return assemblyName is not null
                ? _cache.GetOrAdd(assemblyName, _ =>
                {
                    AssemblyDescriptor? assembly = default;

                    foreach (var root in _roots)
                    {
                        string assenblyFileName = $"{assemblyName}.dll";
                        string assemblyLocation = Path.Combine(root, assenblyFileName);
                        assembly = new AssemblyDescriptor(assemblyLocation);
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
