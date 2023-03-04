namespace Repka.Symbols
{
    public class WorkspaceReference
    {
        public WorkspaceReference(string path)
            : this(Path.GetFileNameWithoutExtension(path), null, path)
        {
        }

        public WorkspaceReference(string name, string? path)
            : this(name, null, path)
        {
        }

        public WorkspaceReference(string name, string? version, string? target)
        {
            Name = name;
            Version = version;
            Target = target;
        }

        public string Name { get; }

        public string? Version { get; }

        public string? Target { get; }

        public bool Exists => Target is not null && File.Exists(Target);

        public override bool Equals(object? obj)
        {
            return obj is WorkspaceReference reference &&
                   Name == reference.Name &&
                   Version == reference.Version &&
                   Target == reference.Target;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version, Target);
        }
    }
}
