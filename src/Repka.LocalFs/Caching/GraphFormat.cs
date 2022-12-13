using Repka.Graphs;

namespace Repka.Caching
{
    public class GraphFormat : ObjectFormat<Graph>
    {
        public override void ReadValue(Stream stream, out Graph? graph)
        {
            graph = new();

            using StreamReader reader = new(stream, leaveOpen: true);

            while(!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    string[] parts = ReadParts(line);
                    if (!parts.Any(string.IsNullOrWhiteSpace))
                    {
                        if (parts.Length == 2)
                        {
                            GraphKey key = new(parts[0]);
                            GraphLabel[] labels = ReadLabels(parts[1]).ToArray();
                            graph.Add(new GraphNodeToken(key, labels));
                        }
                        else if (parts.Length == 3)
                        {
                            GraphKey sourceKey = new(parts[0]);
                            GraphKey targetKey = new(parts[1]);
                            GraphLabel[] labels = ReadLabels(parts[2]).ToArray();
                            graph.Add(new GraphLinkToken(sourceKey, targetKey, labels));
                        }
                    }
                }
            }
        }

        public override void WriteValue(Stream stream, Graph graph)
        {
            using StreamWriter writer = new(stream, leaveOpen: true);

            foreach (var node in graph.Nodes())
            {
                writer.WriteLine(WriteParts(node.Key, WriteLabels(node.Labels)));
            }

            foreach (var link in graph.Links())
            {
                writer.WriteLine(WriteParts(link.SourceKey, link.TargetKey, WriteLabels(link.Labels)));
            }

            writer.Flush();
        }

        private string[] ReadParts(string text)
        {
            return text.Split('\t');
        }


        private string WriteParts(params string[] parts)
        {
            return string.Join("\t", parts);
        }

        private IEnumerable<GraphLabel> ReadLabels(string text)
        {
            return text.Split('|').Select(p => new GraphLabel(p));
        }


        private string WriteLabels(IEnumerable<GraphLabel> labels)
        {
            return string.Join("|", labels);
        }
    }
}
