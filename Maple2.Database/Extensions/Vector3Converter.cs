using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maple2.Database.Extensions;

public class Vector3Converter : JsonConverter<Vector3> {
    private struct Vector3Surrogate {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var surrogate = JsonSerializer.Deserialize<Vector3Surrogate>(ref reader, options);
        return new Vector3(surrogate.X, surrogate.Y, surrogate.Z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options) {
        var surrogate = new Vector3Surrogate { X = value.X, Y = value.Y, Z = value.Z };
        writer.WriteRawValue(JsonSerializer.SerializeToUtf8Bytes(surrogate, options));
    }
}
