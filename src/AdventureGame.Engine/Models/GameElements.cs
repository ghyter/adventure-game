// ==============================
// AdventureGame.Engine/Models/GameElements.cs
// ==============================
using AdventureGame.Engine.Models.Elements;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;


// --- Base element ---
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Scene), "scene")]
[JsonDerivedType(typeof(Item), "item")]
[JsonDerivedType(typeof(Npc), "npc")]
[JsonDerivedType(typeof(Player), "player")]
[JsonDerivedType(typeof(Exit), "exit")]
public abstract class GameElement : IJsonOnDeserialized
{
    public ElementId Id { get; init; } = ElementId.New();
    public string Kind => GetType().Name.ToLowerInvariant();

    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    // Default to off-map
    public Location Location { get; set; } = Location.OffMap();

    // If true, the editor will prevent deletion of this element (protected/core elements)
    public bool CanBeDeleted { get; set; } = true;

    [JsonInclude]
    public HashSet<string> Aliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, int> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, string?> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, bool> Flags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, GameElementState> States { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    protected GameElement()
    {
        Flags.TryAdd(FlagKeys.IsVisible, true);
        Properties.TryAdd(PropertyKeys.DefaultState, "");

        // Ensure a sane default state exists and has a type-appropriate SVG
        EnsureDefaultStateExists();
    }

    private void EnsureDefaultStateExists()
    {
        // Get or create default state name
        if (!Properties.TryGetValue(PropertyKeys.DefaultState, out var ds) || string.IsNullOrWhiteSpace(ds))
        {
            ds = "default";
            Properties[PropertyKeys.DefaultState] = ds;
        }
        else
        {
            ds = ds.Trim();
            Properties[PropertyKeys.DefaultState] = ds;
        }

        // If the States dictionary does not contain an entry for this default state, add one with a type-specific SVG
        if (!States.ContainsKey(ds))
        {
            var svg = GetTypeDefaultSvg();
            States[ds] = new GameElementState("", svg);
        }
    }

    private string GetTypeDefaultSvg()
    {
        // Use simple type-based fallbacks (do not rely on States/DefaultState to avoid recursion)
        var label = System.Security.SecurityElement.Escape(Name ?? "");
        return this switch
        {
            Scene => "<rect width='100' height='100' fill='#1a1a1a' stroke='#333' />",
            Item => $"<circle r='8' fill='#ffcc00'><title>{label}</title></circle>",
            Npc => $"<circle r='8' fill='#ffcc00'><title>{label}</title></circle>",
            Player => $"<circle r='8' fill='#ffcc00'><title>{label}</title></circle>",
            Exit => $"<circle r='8' fill='#ffcc00'><title>{label}</title></circle>",
            _ => "<rect width='100' height='100' fill='#1a1a1a' stroke='#333' />",
        };
    }


    [JsonIgnore]
    public bool IsVisible
    {
        get => Flags.TryGetValue(FlagKeys.IsVisible, out var v) ? v : true;
        set => Flags[FlagKeys.IsVisible] = value;
    }

    [JsonIgnore]
    public string DefaultState
    {
        get => Properties.TryGetValue(PropertyKeys.DefaultState, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v!.Trim()
            : "";
        set => Properties[PropertyKeys.DefaultState] = value?.Trim() ?? "";
    }

    // --- State helpers ---
    public bool TryAddState(string name, string description,string svg, bool setAsDefault = false)
    {
        var key = (name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(key)) return false;
        States[key] = new GameElementState(description ?? "", svg ?? "");
        if (setAsDefault) DefaultState = key;
        return true;
    }

    public bool RemoveStateSafely(string name)
    {
        var key = (name ?? "").Trim();
        if (!States.ContainsKey(key)) return false;
        if (States.Count <= 1) return false;
        if (string.Equals(DefaultState, key, StringComparison.OrdinalIgnoreCase)) return false;
        return States.Remove(key);
    }

    public bool TrySetDefaultState(string name)
    {
        var key = (name ?? "").Trim();
        if (!States.ContainsKey(key)) return false;
        DefaultState = key;
        return true;
    }

    public void SetStates(IEnumerable<KeyValuePair<string, GameElementState>> states, string defaultState)
    {
        States.Clear();
        foreach (var kv in states)
        {
            var key = (kv.Key ?? "").Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("State name cannot be empty.", nameof(states));
            States[key] = kv.Value ?? new GameElementState("", null);
        }
        DefaultState = defaultState?.Trim() ?? "";
        ValidateStatesOrThrow();
    }

    public virtual void OnDeserialized()
    {
        Flags.TryAdd(FlagKeys.IsVisible, true);
        Properties.TryAdd(PropertyKeys.DefaultState, "");
        if (Properties.TryGetValue(PropertyKeys.DefaultState, out var v) && v is not null)
            Properties[PropertyKeys.DefaultState] = v.Trim();

        // Ensure the default state exists after deserialization
        EnsureDefaultStateExists();
    }

    public virtual void ValidateStatesOrThrow()
    {
        if (States.Count == 0)
            throw new InvalidOperationException($"At least one state is required for '{Name}'.");
        if (string.IsNullOrWhiteSpace(DefaultState))
            throw new InvalidOperationException($"DefaultState must be set for '{Name}'.");
        if (!States.ContainsKey(DefaultState))
            throw new InvalidOperationException($"DefaultState '{DefaultState}' must exist for '{Name}'.");
    }

}
