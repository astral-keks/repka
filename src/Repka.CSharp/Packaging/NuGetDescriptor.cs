using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Repka.Packaging
{
    public class NuGetDescriptor
    {
        public static NuGetDescriptor Of(PackageDependency package)
        {
            return Of(package.Id, package.VersionRange?.MaxVersion ?? package.VersionRange?.MinVersion);
        }

        public static NuGetDescriptor Of((string Id, string? Version) package)
        {
            return Of(package.Id, package.Version);
        }

        public static NuGetDescriptor Of(string id, string? version)
        {
            NuGetIdentifier nugetIdentifier = new(id);
            NuGetVersion.TryParse(version, out NuGetVersion? nugetVersion);
            return new(nugetIdentifier, nugetVersion);
        }

        public static NuGetDescriptor Of(string id, NuGetVersion? version)
        {
            NuGetIdentifier nugetIdentifier = new(id);
            return new(nugetIdentifier, version);
        }

        public NuGetDescriptor(NuGetIdentifier id, NuGetVersion? version)
        {
            Id = id;
            Version = version;
        }

        public NuGetIdentifier Id { get; }

        public NuGetVersion? Version { get; set; }

        public static bool operator ==(NuGetDescriptor? left, NuGetDescriptor? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NuGetDescriptor? left, NuGetDescriptor? right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object? obj)
        {
            return obj is NuGetDescriptor descriptor &&
                   Id.Equals(descriptor.Id) &&
                   EqualityComparer<NuGetVersion?>.Default.Equals(Version, descriptor.Version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }

        public override string ToString()
        {
            return $"{Id}:{Version}";
        }
    }
}
