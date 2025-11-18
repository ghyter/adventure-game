// ==============================
// AdventureGame.Engine/Runtime/GameSession.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Infrastructure;
using AdventureGame.Engine.Models;
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
    public List<GameRound> History { get; set; } = new();

    // ---- Construction ----
    private GameSession(GamePack pack)
    {
        GamePackId = pack.Id;
        LoadPack(pack);
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
        

        // Load verbs and triggers
        foreach (var v in pack.Verbs)
            Verbs.Add(v);

        foreach (var t in pack.Triggers)
            Triggers.Add(t);
    }
}
