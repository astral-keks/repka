using NuGet.Packaging.Core;
using NuGet.Versioning;
using Repka.Strings;

namespace Repka.Packaging
{
    public class NuGetDescriptor : Normalizable, IComparable<NuGetDescriptor>
    {
        public static NuGetDescriptor Of(PackageDependency package)
        {
            VersionRange versions = package.VersionRange;
            NuGetVersion? maxVersion = versions.IsMaxInclusive ? versions.MaxVersion : default;
            NuGetVersion? minVersion = versions.IsMinInclusive ? versions.MinVersion : default;
            return Of(package.Id, maxVersion ?? minVersion);
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
            : base($"{id}:{version}")
        {
            Id = id;
            Version = version;
        }

        public NuGetIdentifier Id { get; }

        public NuGetVersion? Version { get; }

        public static bool operator ==(NuGetDescriptor? left, NuGetDescriptor? right) => Equals(left, right);

        public static bool operator !=(NuGetDescriptor? left, NuGetDescriptor? right) => !Equals(left, right);

        public override bool Equals(object? obj)
        {
            return obj is NuGetDescriptor descriptor &&
                Equals(Id, descriptor.Id) &&
                Equals(Version, descriptor.Version);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Version);
        }

        public int CompareTo(NuGetDescriptor? other)
        {
            return string.Compare(Normalized, other?.Normalized);
        }

        public override string ToString()
        {
            return $"{Id}:{Version}";
        }
    }
}
