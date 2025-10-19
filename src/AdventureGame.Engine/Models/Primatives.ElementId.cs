// ==============================
// AdventureGame.Engine/Models/Primitives.ElementId.cs  (NEW OR EDIT EXISTING)
// ==============================
#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;

/// Strong ID value type with JSON converter that supports dictionary keys.
[JsonConverter(typeof(ElementIdJsonConverter))]
public readonly record struct ElementId(string Value)
{
    public static ElementId New() => new(Guid.NewGuid().ToString("n"));
    public static implicit operator string(ElementId id) => id.Value;
    public static implicit operator ElementId(string v) => new(v ?? string.Empty);
}

public sealed class ElementIdJsonConverter : JsonConverter<ElementId>
{
    public override ElementId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? string.Empty);

    public override void Write(Utf8JsonWriter writer, ElementId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);

    // Enable use as DICTIONARY KEY (property name)
    public override ElementId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? string.Empty);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ElementId value, JsonSerializerOptions options)
        => writer.WritePropertyName(value.Value);
}
