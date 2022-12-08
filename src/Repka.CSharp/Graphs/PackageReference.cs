namespace Repka.Graphs
{
    public class PackageReference
    {
        public PackageReference(string packageId, string? packageVersion)
        {
            Id = packageId;
            Version = packageVersion;
        }

        public string Id { get; }

        public string? Version { get; }
    }
}
