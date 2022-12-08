using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Repka.Graphs
{
    public class GraphTokenConverter : JsonConverter<GraphToken>
    {
        public override GraphToken? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override void Write(Utf8JsonWriter writer, GraphToken token, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
