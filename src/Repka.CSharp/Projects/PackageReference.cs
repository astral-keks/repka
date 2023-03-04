namespace Repka.Projects
{
    internal class PackageReference
    {
        public PackageReference(string packageId, string? packageVersion)
        {
            Id = packageId;
            Version = packageVersion;
        }

        public string Id { get; }

        public string? Version { get; }

        public string Name => Version is not null
            ? $"{Id}:{Version}"
            : Id;
    }
}
