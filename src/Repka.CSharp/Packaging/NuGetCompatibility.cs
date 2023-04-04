using NuGet.Frameworks;
using Repka.Optionals;

namespace Repka.Packaging
{
    public class NuGetCompatibility
    {
        private readonly IEnumerable<string> _frameworks;
        private readonly Lazy<CompatibilityTable> _table;

        public NuGetCompatibility(IEnumerable<string> frameworks)
        {
            _frameworks = frameworks;
            _table = new(Build);
        }

        public IOptional<string> Resolve(string? targetFramework)
        {
            NuGetFramework framework = NuGetMoniker.Resolve(targetFramework)?.Framework ?? NuGetFramework.AnyFramework;
            return _table.Value.GetNearest(framework).FirstOrDefault().Moniker().ToOptional();
        }

        private CompatibilityTable Build()
        {
            IEnumerable<NuGetFramework> frameworks = _frameworks
                .Select(label => NuGetMoniker.Resolve(label)?.Framework)
                .OfType<NuGetFramework>();
            CompatibilityTable table = new(frameworks);
            return table;
        }
    }

}
