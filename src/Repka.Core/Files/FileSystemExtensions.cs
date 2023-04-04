namespace Repka.Files
{
    public static class FileSystemExtensions
    {
        public static IEnumerable<DirectoryInfo> ParentDirectories(this FileInfo file)
        {
            if (file.Directory is not null)
            {
                yield return file.Directory;
                foreach (var parent in file.Directory.ParentDirectories())
                    yield return parent;
            }
        }

        public static IEnumerable<DirectoryInfo> ParentDirectories(this DirectoryInfo directory)
        {
            if (directory.Parent is not null)
            {
                yield return directory.Parent;
                foreach (var parent in directory.Parent.ParentDirectories())
                    yield return parent;
            }
        }
    }
}
