using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;

/// <summary>
/// Location of an element — either in world coordinates or embedded under another element.
/// </summary>
[JsonConverter(typeof(LocationJsonConverter))]
public sealed class Location
{
    public GridPosition? Position { get; private set; }   // world position (if set)
    public ElementId? ParentId { get; private set; }      // container or scene (if set)

    private Location() { }

    public static Location World(GridPosition pos) => new() { Position = pos };
    public static Location Embedded(ElementId parentId) => new() { ParentId = parentId };
    public static Location OffMap() => new(); // neither parent nor position

    public bool IsWorld => Position is not null;
    public bool IsEmbedded => ParentId is not null;
    public bool IsOffMap => Position is null && ParentId is null;

    public bool TryGetPosition(out GridPosition pos)
    {
        if (Position is { } p) { pos = p; return true; }
        pos = default; return false;
    }

    public bool TryGetParent(out ElementId parent)
    {
        if (ParentId is { } p) { parent = p; return true; }
        parent = default; return false;
    }

    public override string ToString() =>
        IsWorld ? $"World:{Position}" :
        IsEmbedded ? $"Embedded:{ParentId}" :
        "OffMap";
}

/// <summary>
/// JSON shape examples:
/// { "$loc": "world", "pos": { "x":0,"y":0,"z":0 } }
/// { "$loc": "embedded", "parentId": "..." }
/// { "$loc": "offMap" }
/// </summary>
public sealed class LocationJsonConverter : JsonConverter<Location>
{
    public override Location? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
    {
        if (r.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        string? kind = null;
        GridPosition? pos = null;
        string? parent = null;

        while (r.Read() && r.TokenType != JsonTokenType.EndObject)
        {
            if (r.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            var name = r.GetString();
            r.Read();

            switch (name)
            {
                case "$loc": kind = r.GetString(); break;
                case "pos":
                    if (r.TokenType != JsonTokenType.StartObject) throw new JsonException();
                    int x = 0, y = 0, z = 0;
                    while (r.Read() && r.TokenType != JsonTokenType.EndObject)
                    {
                        if (r.TokenType != JsonTokenType.PropertyName) throw new JsonException();
                        var pn = r.GetString(); r.Read();
                        switch (pn)
                        {
                            case "x": x = r.GetInt32(); break;
                            case "y": y = r.GetInt32(); break;
                            case "z": z = r.GetInt32(); break;
                        }
                    }
                    pos = new GridPosition(x, y, z);
                    break;
                case "parentId":
                    parent = r.GetString();
                    break;
                default:
                    r.Skip();
                    break;
            }
        }

        return kind?.ToLowerInvariant() switch
        {
            "world" => pos is null ? throw new JsonException("world requires pos") : Location.World(pos.Value),
            "embedded" => string.IsNullOrWhiteSpace(parent) ? throw new JsonException("embedded requires parentId") : Location.Embedded(new ElementId(parent!)),
            "offmap" => Location.OffMap(),
            null => Location.OffMap(),
            _ => throw new JsonException($"Invalid $loc '{kind}'")
        };
    }

    public override void Write(Utf8JsonWriter w, Location v, JsonSerializerOptions o)
    {
        w.WriteStartObject();

        if (v.IsWorld)
        {
            w.WriteString("$loc", "world");
            var p = v.Position!.Value;
            w.WritePropertyName("pos");
            w.WriteStartObject();
            w.WriteNumber("x", p.X);
            w.WriteNumber("y", p.Y);
            w.WriteNumber("z", p.Z);
            w.WriteEndObject();
        }
        else if (v.IsEmbedded)
        {
            w.WriteString("$loc", "embedded");
            w.WriteString("parentId", v.ParentId!.Value);
        }
        else
        {
            w.WriteString("$loc", "offMap");
        }

        w.WriteEndObject();
    }
}
