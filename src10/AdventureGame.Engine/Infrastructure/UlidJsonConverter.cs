using NUlid;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Infrastructure;

public sealed class UlidJsonConverter: JsonConverter<Ulid>
{
    public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Expect a JSON string
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("ULID must be a string.");

        var s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s))
            throw new JsonException("ULID string was null or empty.");

        // Be strict: ULID canonical Crockford base32 (26 chars)
        if (Ulid.TryParse(s, out var value))
            return value;

        throw new JsonException($"Invalid ULID: '{s}'.");
    }

    public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
    {
        // Serialize in canonical 26-char representation
        writer.WriteStringValue(value.ToString());
    }

}
