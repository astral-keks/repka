using Repka.Assemblies;

namespace Repka.Frameworks
{
    public class FrameworkDefinition
    {
        private readonly string? _moniker;
        private readonly AssemblyResolver _resolver;
        private readonly List<AssemblyDescriptor> _assemblies;

        public FrameworkDefinition(string? moniker, AssemblyResolver resolver, List<string>? assemblies = null)
            : this(moniker, resolver)
        {
            if (assemblies is not null)
                _assemblies.AddRange(assemblies.Select(resolver.FindAssembly).OfType<AssemblyDescriptor>());
        }

        private FrameworkDefinition(string? moniker, List<AssemblyDescriptor> assemblies, AssemblyResolver resolver)
            : this(moniker, resolver)
        {
            _assemblies.AddRange(assemblies);
        }

        private FrameworkDefinition(string? moniker, AssemblyResolver resolver)
        {
            _moniker = moniker;
            _resolver = resolver;
            _assemblies = new();
        }

        public string? Moniker => _moniker;

        public AssemblyResolver Resolver => _resolver;

        public IReadOnlyCollection<AssemblyDescriptor> Assemblies => _assemblies;
        
        public static FrameworkDefinition operator &(FrameworkDefinition left, FrameworkDefinition right) => left.CombineWith(right);
        public FrameworkDefinition CombineWith(FrameworkDefinition frameworkDefinition)
        {
            return new(_moniker ?? frameworkDefinition._moniker, 
                _assemblies.Concat(frameworkDefinition._assemblies).ToList(),
                new AssemblyResolver(_resolver, frameworkDefinition._resolver));
        }
    }
}
