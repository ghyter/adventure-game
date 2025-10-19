// ==============================
// AdventureGame.Engine/Runtime/GameSession.cs
// ==============================
#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Runtime;

/// <summary>
/// Session instance of a pack element. Holds mutable runtime state while referencing the immutable pack element.
/// </summary>
public sealed class GameSessionElement
{
    /// <summary>Id of the source pack element.</summary>
    public ElementId PackId { get; init; }

    /// <summary>Bound read-only view into the pack's element (set via <see cref="GameSession.BindPack"/>).</summary>
    [JsonIgnore] public IReadOnlyGameElement? Pack { get; private set; }

    /// <summary>Runtime location; initialized from the pack’s location.</summary>
    public Location Location { get; set; } = Location.OffMap();

    /// <summary>Currently active state key; initialized from pack default.</summary>
    public string ActiveState { get; private set; } = "";

    /// <summary>Aliases at runtime.</summary>
    public HashSet<string> Aliases { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Attributes at runtime.</summary>
    public Dictionary<string, int> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Properties at runtime.</summary>
    public Dictionary<string, string?> Properties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Flags at runtime.</summary>
    public Dictionary<string, bool> Flags { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>State name → description (copied for quick lookups/rendering).</summary>
    public Dictionary<string, string> StateDescriptions { get; } = new(StringComparer.OrdinalIgnoreCase);

    public GameSessionElement() { }

    public GameSessionElement(IReadOnlyGameElement pack)
    {
        Pack = pack;
        PackId = pack.Id;

        foreach (var a in pack.Aliases) Aliases.Add(a);
        foreach (var kv in pack.Attributes) Attributes[kv.Key] = kv.Value;
        foreach (var kv in pack.Properties) Properties[kv.Key] = kv.Value;
        foreach (var kv in pack.Flags) Flags[kv.Key] = kv.Value;
        foreach (var kv in pack.StateDescriptions) StateDescriptions[kv.Key] = kv.Value;

        Location = pack.Location;
        ActiveState = pack.DefaultState;
    }

    /// <summary>Re-binds <see cref="Pack"/> from an index and normalizes <see cref="ActiveState"/>.</summary>
    public void Bind(IReadOnlyDictionary<ElementId, IReadOnlyGameElement> index)
    {
        Pack = index.TryGetValue(PackId, out var p) ? p : null;
        if (string.IsNullOrWhiteSpace(ActiveState) || !StateDescriptions.ContainsKey(ActiveState))
            ActiveState = Pack?.DefaultState ?? StateDescriptions.Keys.FirstOrDefault() ?? "";
    }

    /// <summary>Set active state if it exists.</summary>
    public bool TrySetState(string state)
    {
        var s = (state ?? "").Trim();
        if (!StateDescriptions.ContainsKey(s)) return false;
        ActiveState = s;
        return true;
    }
}

/// <summary>
/// Live game session. Contains per-element session overlays and selection of the active player.
/// </summary>
public sealed class GameSession
{
    /// <summary>All session elements keyed by their pack id.</summary>
    public Dictionary<ElementId, GameSessionElement> Elements { get; } = new();

    /// <summary>Currently selected player id, if any.</summary>
    public ElementId? ActivePlayerId { get; private set; }

    /// <summary>Convenience accessor for the selected player's session element.</summary>
    [JsonIgnore]
    public GameSessionElement? ActivePlayerSe
        => ActivePlayerId is { } id && Elements.TryGetValue(id, out var se) ? se : null;

    /// <summary>Index of pack elements used for binding; not serialized.</summary>
    [JsonIgnore]
    public IReadOnlyDictionary<ElementId, IReadOnlyGameElement> PackIndex { get; private set; }
        = new Dictionary<ElementId, IReadOnlyGameElement>();

    public GameSession() { }

    /// <summary>Create a session from a validated pack. Optionally auto-selects the single player candidate.</summary>
    public static GameSession FromPack(GamePack pack, bool autoSelectSinglePlayer = true)
    {
        var s = new GameSession();

        var ro = pack.AsReadOnly().ToList();
        var idx = ro.ToDictionary(e => e.Id, e => e);
        foreach (var e in ro) s.Elements[e.Id] = new GameSessionElement(e);
        s.PackIndex = idx;

        if (autoSelectSinglePlayer && pack.PlayerCandidates.Count == 1)
            s.ActivePlayerId = pack.PlayerCandidates[0].Id;

        return s;
    }

    /// <summary>Rebinds session elements to the provided pack view (e.g., after reload).</summary>
    public void BindPack(IEnumerable<IReadOnlyGameElement> pack)
    {
        var idx = pack.ToDictionary(e => e.Id, e => e);
        PackIndex = idx;
        foreach (var se in Elements.Values) se.Bind(PackIndex);
    }

    /// <summary>Selects the active player by id if the id is a valid <see cref="Player"/> element.</summary>
    public bool SelectPlayerById(ElementId id)
    {
        if (!Elements.ContainsKey(id)) return false;
        if (!PackIndex.TryGetValue(id, out var ge)) return false;
        if (ge is not Player) return false;
        ActivePlayerId = id;
        return true;
    }

    /// <summary>Gets a session element by id; null if missing.</summary>
    public GameSessionElement? Get(ElementId id)
        => Elements.TryGetValue(id, out var se) ? se : null;

    /// <summary>Serialize the session. PackIndex is not included.</summary>
    public string ToJson(JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(this, options ?? new JsonSerializerOptions { WriteIndented = true });

    /// <summary>Deserialize a session. Call <see cref="BindPack"/> afterwards.</summary>
    public static GameSession FromJson(string json, JsonSerializerOptions? options = null)
        => JsonSerializer.Deserialize<GameSession>(json, options ?? new JsonSerializerOptions()) ?? new GameSession();
}
