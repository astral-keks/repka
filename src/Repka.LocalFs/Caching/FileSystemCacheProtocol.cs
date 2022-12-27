using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Caching
{
    internal static class FileSystemCacheProtocol
    {
        public const string EntryElement = "<Entry>";
        public const string PropertyElement = "<Property>";
        public const string BeginContentElement = "<Content>"; 
        public const string EndContentElement = "</Content>"; 
    }
}
