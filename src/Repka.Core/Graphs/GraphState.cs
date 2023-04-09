using System.Collections.Concurrent;

namespace Repka.Graphs
{
    public class GraphState
    {
        private readonly ConcurrentDictionary<string, GraphAttribute> _attributes = new();

        public void Set(GraphAttribute attribute)
        {
            _attributes.AddOrUpdate(attribute.Name, attribute, (_, _) => attribute);
        }

        public GraphAttribute Attribute(string name)
        {
            return _attributes.GetOrAdd(name, _ => new(name));
        }
    }
}
