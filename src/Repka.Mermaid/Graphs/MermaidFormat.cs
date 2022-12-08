using Repka.Optionals;
using System.Text;

namespace Repka.Graphs
{
    public class MermaidFormat
    {
        public GraphKey? RootKey { get; set; }

        public int MaxDepth { get; set; } = 1;

        public bool WrapForMarkdown { get; set; } = true;

        public string ToString(Graph graph)
        {
            StringBuilder builder = new();
            if (WrapForMarkdown)
                builder.AppendLine("```mermaid");

            builder.AppendLine("erDiagram");
            if (RootKey is not null)
            {
                Dictionary<GraphKey, GraphNode> nodes = graph.Node(RootKey).ToOptional()
                    .SelectMany(root => root.TraverseBreadth(MaxDepth))
                    .ToDictionary(node => node.Key);
                List<GraphLink> links = nodes.Values
                    .SelectMany(node => node.Links())
                    .Where(link => nodes.ContainsKey(link.TargetKey) && nodes.ContainsKey(link.SourceKey))
                    .ToList();

                foreach (var link in links)
                {
                    builder.AppendLine(ToString(link));
                }
            }

            if (WrapForMarkdown)
                builder.AppendLine("```");

            return builder.ToString();
        }

        public string ToString(GraphNode node)
        {
            return $"class {ToString(node.Key)}";
        }

        public string ToString(GraphLink link)
        {
            StringBuilder builder = new();

            string relation = link.Source() is not null && link.Target() is not null ? "||--o{" : "||..o{";
            builder.Append($"{ToString(link.SourceKey)} {relation} {ToString(link.TargetKey)}");
            if (link.Attribute<object?>()?.Value is object value)
                builder.Append($" : {value.GetType().Name}");

            return builder.ToString();
        }

        public string ToString(GraphKey key)
        {
            return $"{Path.GetFileName(key)}".Replace(".", "_");
        }
    }
}