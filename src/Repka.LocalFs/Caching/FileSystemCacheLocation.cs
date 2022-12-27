using System.Security.Cryptography;
using System.Text;

namespace Repka.Caching
{
    internal class FileSystemCacheLocation
    {
        private readonly string _cacheName;

        public FileSystemCacheLocation(string cacheName)
        {
            _cacheName = cacheName;
        }

        public string FullName(string root)
        {
            return Path.Combine(root, Name);
        }

        public string Name
        {
            get
            {
                using MD5 md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(_cacheName));
                return Convert.ToHexString(hash);
            }
        }
    }
}
