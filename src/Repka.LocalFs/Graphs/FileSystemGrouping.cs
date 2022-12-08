using System.Collections;

namespace Repka.Graphs
{
    internal class FileSystemGrouping : IEnumerable<(string Directory, HashSet<GraphKey> Items)>
    {
        private readonly Dictionary<string, HashSet<GraphKey>> _keysByDirectory = new();

        public void Add(GraphKey key)
        {
            string? directory = Path.GetDirectoryName(key);
            if (directory is not null)
            {
                if (!_keysByDirectory.ContainsKey(directory))
                    _keysByDirectory[directory] = new();
                _keysByDirectory[directory].Add(key);
            }
        }

        public IEnumerator<(string Directory, HashSet<GraphKey> Items)> GetEnumerator()
        {
            return _keysByDirectory.Select(entry => (entry.Key, entry.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
