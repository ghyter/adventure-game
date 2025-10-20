// ==============================
// AdventureGame.Engine/Models/GamePack.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Infrastructure;
using NUlid;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AdventureGame.Engine.Models;

/// <summary>
/// Authoring-time definition of a complete game world.
/// This object is mutable and safe for editing or serialization.
/// </summary>
public sealed class GamePack
{
    // ---- Identity ----
    [JsonConverter(typeof(UlidJsonConverter))]
    public Ulid Id { get; set; } = Ulid.NewUlid();

    // ---- Metadata ----
    public string Name { get; set; } = "Untitled Game";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "";

    // ---- World/Grid ----
    public GridConfig Grid { get; set; } = new();

    // ---- Content ----
    public List<GameElement> Elements { get; } = new();
    public GamePackVfs Vfs { get; } = new();


    public List<Verb> Verbs { get; } = new();
    public List<Trigger> Triggers { get; } = new();

    // ---- Metadata ----
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // ---- JSON Helpers ----
    public static JsonSerializerOptions JsonOptions => new()
    {
        WriteIndented = true,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static GamePack FromJson(string json)
        => JsonSerializer.Deserialize<GamePack>(json, JsonOptions)
           ?? throw new InvalidOperationException("Failed to deserialize GamePack.");

    public GamePack Clone()
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        return JsonSerializer.Deserialize<GamePack>(json, JsonOptions)!;
    }

}
