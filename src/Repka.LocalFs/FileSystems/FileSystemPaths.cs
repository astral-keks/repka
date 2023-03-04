using System.Security.Cryptography;
using System.Text;

namespace Repka.FileSystems
{
    internal static class FileSystemPaths
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

        public static string GetRootPath(string? root, string store)
        {
            if (root is not null)
            {
                using MD5 md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(store));
                string directory = Convert.ToHexString(hash);

                return Path.Combine(root, directory);
            }
            else
                return Path.Combine(store, ".repka");
        }
    }
}
