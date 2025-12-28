// ==============================
// AdventureGame.Engine/Runtime/GameSession.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Serialization;
using AdventureGame.Engine.Models.Elements;
using NUlid;
using System.Text.Json.Serialization;
using AdventureGame.Engine.Models.Actions;

namespace AdventureGame.Engine.Models.Runtime;

/// <summary>
/// A live, mutable instance of a loaded GamePack.
/// Manages game elements, verbs, and triggers during play.
/// </summary>
public sealed class GameSession
{
    // ---- Identity ----
    [JsonConverter(typeof(UlidJsonConverter))]
    public Ulid SessionId { get; init; } = Ulid.NewUlid();

    [JsonConverter(typeof(UlidJsonConverter))]
    public Ulid GamePackId { get; init; }

    // ---- State ----
    public List<GameElement> Elements { get; } = [];
    public List<GameAction> Actions { get; set; } = [];
    public List<GameRound> History { get; set; } = [];

    // ---- Game Reference ----
    [JsonIgnore]
    public GamePack? Pack { get; private set; }

    // ---- Game State ----
    [JsonIgnore]
    public GameElement? Player { get; set; }

    [JsonIgnore]
    public GameElement? CurrentTarget { get; set; }

    [JsonIgnore]
    public Scene? CurrentScene { get; set; }

    // ---- Construction ----
    private GameSession(GamePack pack)
    {
        GamePackId = pack.Id;
        Pack = pack;
        LoadPack(pack);
    }

    public static GameSession NewGame(GamePack pack)
        => new(pack);

    // ---- Load from GamePack ----
    private void LoadPack(GamePack pack)
    {
        Elements.Clear();
        History.Clear();

        // Clone or reference elements
        foreach (var e in pack.Elements)
        {
            Elements.Add(e);
        }
        
        // Find the player element
        Player = Elements.FirstOrDefault(e => e.Kind == "player");

        // Find the default scene
        CurrentScene = Elements.OfType<Scene>().FirstOrDefault();

    }
}
