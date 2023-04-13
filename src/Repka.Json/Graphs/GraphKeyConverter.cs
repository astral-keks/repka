using System.Text.Json;
using System.Text.Json.Serialization;

namespace Repka.Graphs
{
    public class GraphKeyConverter : JsonConverter<GraphKey>
    {
        public override GraphKey? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? resource = JsonSerializer.Deserialize<string>(ref reader, options);
            return resource is not null ? new GraphKey(resource) : null;
        }

        public override void Write(Utf8JsonWriter writer, GraphKey key, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, key.ToString(), options);
        }
    }
}
