// ==============================
// AdventureGame.Engine/Runtime/GameSession.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Infrastructure;
using AdventureGame.Engine.Models;
using NUlid;
using System.Text.Json.Serialization;
using AdventureGame.Engine.Extensions;

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
    public Dictionary<ElementId, GameElement> Elements { get; } = new();
    public Dictionary<string, Verb> Verbs { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, Trigger> Triggers { get; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    private readonly Dictionary<string, ElementId> _aliasIndex = new(StringComparer.OrdinalIgnoreCase);

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
        _aliasIndex.Clear();
        Verbs.Clear();
        Triggers.Clear();

        // Clone or reference elements
        foreach (var e in pack.Elements)
        {
            Elements[e.Id] = e;
            _aliasIndex[e.Name] = e.Id;
            foreach (var a in e.Aliases)
                _aliasIndex[a] = e.Id;
        }

        // Load verbs and triggers
        foreach (var v in pack.Verbs)
            Verbs[v.Name] = v;

        foreach (var t in pack.Triggers)
            Triggers[t.Name] = t;
    }

    // ---- Lookups ----
    public GameElement? FindById(ElementId id)
        => Elements.TryGetValue(id, out var e) ? e : null;

    public GameElement? FindByNameOrAlias(string name)
        => _aliasIndex.TryGetValue(name, out var id) ? Elements[id] : null;

    public Verb? FindVerb(string name)
        => Verbs.TryGetValue(name, out var v) ? v : null;

    public Trigger? FindTrigger(string name)
        => Triggers.TryGetValue(name, out var t) ? t : null;

    // ---- Runtime helpers ----
    public IEnumerable<Trigger> GetActiveTriggers()
        => Triggers.Values.Where(t => t.Conditions?.Evaluate(this) ?? false);

    public void ApplyEffects(IEnumerable<GameEffect> effects)
    {
        foreach (var fx in effects)
            fx.Apply(this);
    }
}
