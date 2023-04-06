using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;

namespace Repka.Workspaces
{
    internal class WorkspaceReferences
    {
        private readonly ConcurrentDictionary<string, ICollection<MetadataReference>> _references = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, string> _unresolved = new(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> Unresolved => _unresolved.Keys;

        public ICollection<MetadataReference> GetOrAdd(string key, Func<MetadataReference> factory) =>
            _references.GetOrAdd(key, _ => new List<MetadataReference>(1) { factory() });

        public ICollection<MetadataReference> GetOrAdd(string key, Func<IEnumerable<MetadataReference>?> factory) =>
            _references.GetOrAdd(key, _ => Verify(key, factory()?.ToList() ?? new(0)));

        private ICollection<MetadataReference> Verify(string key, ICollection<MetadataReference> references)
        {
            if (!references.Any())
                _unresolved.GetOrAdd(key, key);
            return references;
        }
    }
}
