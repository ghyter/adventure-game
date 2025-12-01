// ==============================
// AdventureGame.Engine/Models/GamePack.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Infrastructure;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Verbs;
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
    [JsonInclude]
    public List<GameElement> Elements { get; set; } = [];


    // ---- NEW: Unified Actions (replaces Verbs and Triggers) ----
    
    /// <summary>
    /// Unified collection of both verbs (player-initiated) and triggers (automatic).
    /// This replaces the separate Verbs and Triggers collections.
    /// </summary>
    [JsonInclude]
    public List<Actions.GameAction> Actions { get; set; } = [];

    // ---- LEGACY: Old Verbs and Triggers (marked obsolete for migration) ----
    
    /// <summary>
    /// OBSOLETE: Use Actions collection instead.
    /// Kept for backward compatibility and migration.
    /// </summary>
    [JsonInclude]
    [Obsolete("Use Actions collection instead. This property is kept for backward compatibility.")]
    public List<Verb> Verbs { get; set; } = [];
    
    /// <summary>
    /// OBSOLETE: Use Actions collection instead.
    /// Kept for backward compatibility and migration.
    /// </summary>
    [JsonInclude]
    [Obsolete("Use Actions collection instead. This property is kept for backward compatibility.")]
    public List<GameTrigger> Triggers { get; set; } = [];

    // ---- Metadata ----
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Default constructor: ensure a minimal, editable pack with a starting scene, level and player
    public GamePack()
    {
        // Create default level and protect from deletion
        var defaultLevel = new Level
        {
            Name = "Default Level",
            CanBeDeleted = false
        };

        // Create default start scene at origin (0,0) on the default level and protect from deletion
        var startScene = new Scene
        {
            Name = "Start",
            Position = GridPosition.Origin,
            ParentId = defaultLevel.Id,
            CanBeDeleted = false
        };

        // Create default player element (required) and protect from deletion
        var player = new Player
        {
            Name = "player",
            ParentId = startScene.Id,
            CanBeDeleted = false
        };

        Elements.Add(defaultLevel);
        Elements.Add(startScene);
        Elements.Add(player);
    }

    // ---- JSON Helpers ----
    public static JsonSerializerOptions JsonOptions
    {
        get
        {
            var opts = new JsonSerializerOptions()
            {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            };
            // Serialize enums as camel-case strings and disallow integer enum values
            opts.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
            return opts;
        }
    }

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
