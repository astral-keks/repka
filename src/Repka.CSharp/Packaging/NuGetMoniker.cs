using NuGet.Frameworks;

namespace Repka.Packaging
{
    public record class NuGetMoniker
    {
        public static IEnumerable<NuGetMoniker> All =>
            NuGetMonikers.All;

        public static NuGetMoniker? Resolve(string? moniker) => 
            moniker is not null && NuGetMonikers.ById.ContainsKey(moniker) ? NuGetMonikers.ById[moniker] : default;

        public static NuGetMoniker? Resolve(NuGetFramework? framework) => 
            framework is not null && NuGetMonikers.ByFramework.ContainsKey(framework) ? NuGetMonikers.ByFramework[framework] : default;

        public NuGetMoniker(string id) : this(id, NuGetFramework.Parse(id)) { }

        private NuGetMoniker(string id, NuGetFramework framework)
        {
            Id = id;
            Framework = framework;
        }

        public string Id { get; }

        public NuGetFramework Framework { get; }

        public IEnumerable<NuGetMoniker> CompatibilityMatches => NuGetMonikers.CompatibilityGroups[Framework]; 
    }
}
