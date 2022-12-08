using System.Text.Json;
using System.Text.Json.Serialization;

namespace Repka.Graphs
{
    public class GraphElementConverter : JsonConverter<GraphElement>
    {
        public override GraphNode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, GraphElement element, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(element.Token, options);
        }
    }
}
