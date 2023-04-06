namespace Repka.Assemblies
{
    public sealed record class AssemblyFile
    {
        private readonly string _path;

        public AssemblyFile(string path)
        {
            _path = path;
        }

        public string Path => _path;

        public bool Exists => File.Exists(Path);

        public bool Equals(AssemblyFile? file)
        {
            return Path.Equals(file?.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
