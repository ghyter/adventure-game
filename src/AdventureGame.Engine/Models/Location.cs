
// ==============================
// File: AdventureGame.Engine.Models/Location.cs (NEW)
// ==============================
#nullable enable
using AdventureGame.Engine.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;

public enum LocationKind { World, Embedded, Special }
public enum SpecialPlace { OffMap, Inventory }

/// Discriminated location: world cell, embedded under parent element, or special place.
[JsonConverter(typeof(LocationJsonConverter))]
public sealed class Location
{
    public LocationKind Kind { get; private set; }
    public GridPosition? Position { get; private set; }   // when Kind=World
    public ElementId? ParentId { get; private set; }      // when Kind=Embedded
    public SpecialPlace? Special { get; private set; }    // when Kind=Special

    private Location() { }

    public static Location World(GridPosition pos) => new() { Kind = LocationKind.World, Position = pos };
    public static Location Embedded(ElementId parentId) => new() { Kind = LocationKind.Embedded, ParentId = parentId };
    public static Location SpecialOf(SpecialPlace place) => new() { Kind = LocationKind.Special, Special = place };
    public static Location OffMap() => SpecialOf(SpecialPlace.OffMap);
    public static Location Inventory() => SpecialOf(SpecialPlace.Inventory);

    public bool IsWorld => Kind == LocationKind.World;
    public bool IsEmbedded => Kind == LocationKind.Embedded;
    public bool IsSpecial => Kind == LocationKind.Special;

    public bool TryGetPosition(out GridPosition pos) { if (Position is { } p) { pos = p; return true; } pos = default; return false; }
    public bool TryGetParent(out ElementId parent) { if (ParentId is { } p) { parent = p; return true; } parent = default; return false; }
    public bool TryGetSpecial(out SpecialPlace sp) { if (Special is { } s) { sp = s; return true; } sp = default; return false; }

    public override string ToString() => Kind switch
    {
        LocationKind.World => $"World:{Position}",
        LocationKind.Embedded => $"Embedded:{ParentId}",
        LocationKind.Special => $"Special:{Special}",
        _ => "Unknown"
    };
}

/// JSON shape examples:
/// { "$loc":"world",   "pos":{ "x":0,"y":0,"z":0 } }
/// { "$loc":"embedded","parentId":"..." }
/// { "$loc":"special", "place":"offMap"|"inventory" }
public sealed class LocationJsonConverter : JsonConverter<Location>
{
    public override Location? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
    {
        if (r.TokenType != JsonTokenType.StartObject) throw new JsonException();
        string? kind = null; GridPosition? pos = null; string? parent = null; SpecialPlace? sp = null;

        while (r.Read() && r.TokenType != JsonTokenType.EndObject)
        {
            if (r.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            var name = r.GetString(); r.Read();
            switch (name)
            {
                case "$loc": kind = r.GetString(); break;
                case "pos":
                    if (r.TokenType != JsonTokenType.StartObject) throw new JsonException();
                    int x = 0, y = 0, z = 0; bool sx = false, sy = false, sz = false;
                    while (r.Read() && r.TokenType != JsonTokenType.EndObject)
                    {
                        if (r.TokenType != JsonTokenType.PropertyName) throw new JsonException();
                        var pn = r.GetString(); r.Read();
                        switch (pn) { case "x": x = r.GetInt32(); sx = true; break; case "y": y = r.GetInt32(); sy = true; break; case "z": z = r.GetInt32(); sz = true; break; }
                    }
                    if (!(sx && sy && sz)) throw new JsonException("pos requires x,y,z");
                    pos = new GridPosition(x, y, z); break;
                case "parentId": parent = r.GetString(); break;
                case "place":
                    var s = r.GetString();
                    sp = s?.ToLowerInvariant() switch
                    { "offmap" => SpecialPlace.OffMap, "inventory" => SpecialPlace.Inventory, _ => throw new JsonException($"Unknown special place '{s}'") };
                    break;
                default: r.Skip(); break;
            }
        }

        return kind?.ToLowerInvariant() switch
        {
            "world" => pos is null ? throw new JsonException("world requires pos") : Location.World(pos.Value),
            "embedded" => string.IsNullOrWhiteSpace(parent) ? throw new JsonException("embedded requires parentId") : Location.Embedded(new ElementId(parent!)),
            "special" => sp is null ? throw new JsonException("special requires place") : Location.SpecialOf(sp.Value),
            _ => throw new JsonException("Invalid $loc")
        };
    }

    public override void Write(Utf8JsonWriter w, Location v, JsonSerializerOptions o)
    {
        w.WriteStartObject();
        switch (v.Kind)
        {
            case LocationKind.World:
                w.WriteString("$loc", "world");
                var p = v.Position!.Value; w.WritePropertyName("pos"); w.WriteStartObject();
                w.WriteNumber("x", p.X); w.WriteNumber("y", p.Y); w.WriteNumber("z", p.Z); w.WriteEndObject();
                break;
            case LocationKind.Embedded:
                w.WriteString("$loc", "embedded");
                w.WriteString("parentId", v.ParentId!.Value);
                break;
            case LocationKind.Special:
                w.WriteString("$loc", "special");
                var place = v.Special!.Value switch { SpecialPlace.OffMap => "offMap", SpecialPlace.Inventory => "inventory", _ => "unknown" };
                w.WriteString("place", place);
                break;
        }
        w.WriteEndObject();
    }
}
