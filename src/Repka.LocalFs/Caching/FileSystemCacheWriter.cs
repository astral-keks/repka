using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repka.Caching
{
    internal static class FileSystemCacheWriter
    {
        public static void WriteEntries(this StreamWriter writer, IEnumerable<CacheEntry> entries)
        {
            foreach (var entry in entries) 
            { 
                writer.WriteEntry(entry);
            }
        }

        private static void WriteEntry(this StreamWriter writer, CacheEntry entry)
        {
            writer.WriteLine(FileSystemCacheProtocol.EntryElement);
            writer.WriteLine(entry.Key.ToString());
            foreach (var property in entry.Properties)
            {
                writer.WriteProperty(property);
            }
            writer.WriteContent(entry.Content);
        }

        private static void WriteProperty(this StreamWriter writer, CacheProperty property)
        {
            writer.WriteLine(FileSystemCacheProtocol.PropertyElement);
            writer.WriteLine(property.Name);
            writer.WriteLine(property.Value);
        }

        private static void WriteContent(this StreamWriter writer, CacheContent content)
        {
            writer.WriteLine(FileSystemCacheProtocol.BeginContentElement);
            foreach (var line in content.Lines)
            {
                writer.WriteLine(line);
            }
            writer.WriteLine(FileSystemCacheProtocol.EndContentElement);
        }
    }
}
