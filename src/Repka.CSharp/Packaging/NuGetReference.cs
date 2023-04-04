using NuGet.Frameworks;

namespace Repka.Packaging
{
    public static class NuGetReference
    {
        public static NuGetReference<TTarget> Of<TTarget>(NuGetFramework framework, TTarget? target = default)
        {
            return new(framework, target);
        }
    }

    public class NuGetReference<TTarget>
    {
        public NuGetReference(NuGetFramework framework, TTarget? target = default)
        {
            Framework = framework;
            Target = target;
        }

        public NuGetFramework Framework { get; }

        public TTarget? Target { get; }
    }
}
