using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Caching
{
    internal class FileStorageEntry
    {
        private readonly string _key;

        public FileStorageEntry(string key)
        {
            _key = key;
        }

        public string Path(string root)
        {
            return System.IO.Path.Combine(root, Name);
        }

        public string Name
        {
            get
            {
                using MD5 md5 = MD5.Create();
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(_key));
                return Convert.ToHexString(hash);
            }
        }
    }
}
