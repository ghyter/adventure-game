// ==============================
// AdventureGame.Engine/Runtime/GameSession.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Infrastructure;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.DSL;
using NUlid;
using System.Text.Json.Serialization;
using AdventureGame.Engine.Extensions;
using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Models.Actions;

namespace AdventureGame.Engine.Runtime;

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
    public List<Verb> Verbs { get; } = [];
    public List<GameTrigger> Triggers { get; } = [];
    public List<GameRound> History { get; set; } = [];

    // ---- Game Reference ----
    [JsonIgnore]
    public GamePack? Pack { get; private set; }

    // ---- DSL Service ----
    [JsonIgnore]
    public DslService? DslService { get; private set; }

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
        InitializeDslService();
    }

    public static GameSession NewGame(GamePack pack)
        => new(pack);

    // ---- Load from GamePack ----
    private void LoadPack(GamePack pack)
    {
        Elements.Clear();
        Verbs.Clear();
        Triggers.Clear();
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

        // Load verbs and triggers
        foreach (var v in pack.Verbs)
            Verbs.Add(v);

        foreach (var t in pack.Triggers)
            Triggers.Add(t);
    }

    /// <summary>
    /// Initializes the DSL service with vocabulary and canonicalizer from the GamePack.
    /// </summary>
    private void InitializeDslService()
    {
        if (Pack == null) return;

        var vocab = DslVocabulary.FromGamePack(Pack);
        var canonicalizer = new DslCanonicalizer();
        DslService = new DslService(vocab, canonicalizer);
    }
}
