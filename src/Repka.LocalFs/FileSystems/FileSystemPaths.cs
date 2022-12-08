namespace Repka.FileSystems
{
    public static class FileSystemPaths
    {
        public static string? GetParentPath(string path)
        {
            try
            {
                return Path.GetDirectoryName(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
