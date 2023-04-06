using NuGet.Frameworks;

namespace Repka.Packaging
{
    internal static class NuGetMonikers
    {
        public static string? Moniker(this NuGetFramework? framework) => NuGetMoniker.Resolve(framework)?.Id;
        public static IEnumerable<string> Monikers(this IEnumerable<NuGetFramework> frameworks) => frameworks
            .Select(framework => framework.Moniker())
            .OfType<string>();

        public static readonly NuGetMoniker NetCoreApp10 = new("netcoreapp1.0");
        public static readonly NuGetMoniker NetCoreApp11 = new("netcoreapp1.1");
        public static readonly NuGetMoniker NetCoreApp20 = new("netcoreapp2.0");
        public static readonly NuGetMoniker NetCoreApp21 = new("netcoreapp2.1");
        public static readonly NuGetMoniker NetCoreApp22 = new("netcoreapp2.2");
        public static readonly NuGetMoniker NetCoreApp30 = new("netcoreapp3.0");
        public static readonly NuGetMoniker NetCoreApp31 = new("netcoreapp3.1");
        public static readonly NuGetMoniker NetStandard10 = new("netstandard1.0");
        public static readonly NuGetMoniker NetStandard11 = new("netstandard1.1");
        public static readonly NuGetMoniker NetStandard12 = new("netstandard1.2");
        public static readonly NuGetMoniker NetStandard13 = new("netstandard1.3");
        public static readonly NuGetMoniker NetStandard14 = new("netstandard1.4");
        public static readonly NuGetMoniker NetStandard15 = new("netstandard1.5");
        public static readonly NuGetMoniker NetStandard16 = new("netstandard1.6");
        public static readonly NuGetMoniker NetStandard20 = new("netstandard2.0");
        public static readonly NuGetMoniker NetStandard21 = new("netstandard2.1");
        public static readonly NuGetMoniker Net11 = new("net11");
        public static readonly NuGetMoniker Net20 = new("net20");
        public static readonly NuGetMoniker Net35 = new("net35");
        public static readonly NuGetMoniker Net40 = new("net40");
        public static readonly NuGetMoniker Net403 = new("net403");
        public static readonly NuGetMoniker Net45 = new("net45");
        public static readonly NuGetMoniker Net451 = new("net451");
        public static readonly NuGetMoniker Net452 = new("net452");
        public static readonly NuGetMoniker Net46 = new("net46");
        public static readonly NuGetMoniker Net461 = new("net461");
        public static readonly NuGetMoniker Net462 = new("net462");
        public static readonly NuGetMoniker Net47 = new("net47");
        public static readonly NuGetMoniker Net471 = new("net471");
        public static readonly NuGetMoniker Net472 = new("net472");
        public static readonly NuGetMoniker Net48 = new("net48");
        public static readonly NuGetMoniker Net50 = new("net50");
        public static readonly NuGetMoniker Net60 = new("net60");
        public static readonly NuGetMoniker Net70 = new("net70");
        public static readonly NuGetMoniker Any = new("any");
        public static readonly NuGetMoniker[] All =
        {
            NetCoreApp10,
            NetCoreApp11,
            NetCoreApp20,
            NetCoreApp21,
            NetCoreApp22,
            NetCoreApp30,
            NetCoreApp31,
            NetStandard10,
            NetStandard11,
            NetStandard12,
            NetStandard13,
            NetStandard14,
            NetStandard15,
            NetStandard16,
            NetStandard20,
            NetStandard21,
            Net11,
            Net20,
            Net35,
            Net40,
            Net403,
            Net45,
            Net451,
            Net452,
            Net46,
            Net461,
            Net462,
            Net47,
            Net471,
            Net472,
            Net48,
            Net50,
            Net60,
            Net70,
            Any
        };

        public static readonly NuGetFramework[] AllFrameworks = All.Select(moniker => moniker.Framework).ToArray();

        public static readonly Dictionary<string, NuGetMoniker> ById = All.ToDictionary(moniker => moniker.Id);
        public static readonly Dictionary<NuGetFramework, NuGetMoniker> ByFramework = All.ToDictionary(moniker => moniker.Framework);
        internal static readonly CompatibilityTable CompatibilityTable = new(ByFramework.Keys);
        internal static readonly Dictionary<NuGetFramework, IEnumerable<NuGetMoniker>> CompatibilityGroups = All
            .SelectMany(mapping => CompatibilityTable.TryGetCompatible(mapping.Framework, out var compatible)
                ? compatible.Select(compatible => (From: compatible, To: mapping))
                : Enumerable.Empty<(NuGetFramework From, NuGetMoniker To)>())
            .GroupBy(mapping => mapping.From, mapping => mapping.To)
            .ToDictionary(group => group.Key, group => group.ToHashSet().AsEnumerable());
    }
}
