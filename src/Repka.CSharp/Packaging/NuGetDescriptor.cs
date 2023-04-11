using NuGet.Packaging.Core;
using NuGet.Versioning;
using Repka.Strings;

namespace Repka.Packaging
{
    public class NuGetDescriptor : Normalizable, IComparable<NuGetDescriptor>
    {
        public static NuGetDescriptor Parse(string value)
        {
            NuGetDescriptor descriptor;

            string[] parts = value.Split(':');
            if (parts.Length == 2)
                descriptor = new(parts[0], parts[1]);
            else
                descriptor = new(value, default(string));

            return descriptor;
        }

        public static NuGetDescriptor Of(PackageDependency package)
        {
            VersionRange versions = package.VersionRange;
            NuGetVersion? maxVersion = versions.IsMaxInclusive ? versions.MaxVersion : default;
            NuGetVersion? minVersion = versions.IsMinInclusive ? versions.MinVersion : default;
            return new(package.Id, maxVersion ?? minVersion);
        }

        public NuGetDescriptor(string id, string? version)
            : this(id, NuGetVersion.TryParse(version, out NuGetVersion? nugetVersion) ? nugetVersion : default)
        {
        }

        public NuGetDescriptor(string id, NuGetVersion? version)
            : this(new NuGetIdentifier(id), version)
        {
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
