namespace Repka.Graphs
{
    internal class PackageKey : GraphKey
    {
        public PackageKey(string packageId)
            : base(packageId)
        {
        }

        public PackageKey(string packageId, string? packageVersion)
            : base($"{packageId}.{packageVersion ?? "undefined"}")
        {
        }
    }
}
