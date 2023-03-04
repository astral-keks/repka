namespace Repka.Gac
{
    public class GacDirectory
    {
        private readonly List<DirectoryInfo> _roots;

        public GacDirectory(IEnumerable<string> roots)
        {
            _roots = roots.Select(root => new DirectoryInfo(root)).ToList();
        }

        public IEnumerable<FileInfo> ResolveAssembly(string assemblyName)
        {
            List<FileInfo> libraries = new(0);

            foreach (var root in _roots)
            {
                libraries = root.EnumerateFiles($"{assemblyName}.dll").ToList();
                if (libraries.Any())
                    break;

                DirectoryInfo? assemblyDirectory = root.EnumerateDirectories(assemblyName).FirstOrDefault();
                DirectoryInfo? versionDirectory = ResolveVersion(assemblyDirectory);
                libraries = ResolveLibraries(versionDirectory).ToList();
                if (libraries.Any())
                    break;
            }

            return libraries;
        }

        private DirectoryInfo? ResolveVersion(DirectoryInfo? assemblyDirectory)
        {
            DirectoryInfo? versionDirectory = default;

            if (assemblyDirectory is not null)
            {
                versionDirectory = assemblyDirectory.EnumerateDirectories().LastOrDefault();
            }

            return versionDirectory;
        }

        private IEnumerable<FileInfo> ResolveLibraries(DirectoryInfo? versionDirectory)
        {
            if (versionDirectory is not null)
            {
                IEnumerable<FileInfo> libraries = versionDirectory.EnumerateFiles("*.dll");
                foreach (var library in libraries)
                {
                    yield return library;
                }
            }
        }
    }
}
