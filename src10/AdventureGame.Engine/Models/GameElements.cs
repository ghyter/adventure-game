// ==============================
// AdventureGame.Engine/Models/GameElements.cs
// ==============================
using AdventureGame.Engine.Helpers;
using AdventureGame.Engine.Models.Elements;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;


// --- Base element ---
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Level), "level")]
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

    // ParentId directly on element; for Scene this is a Level id; nullable for OffMap
    public ElementId? ParentId { get; set; }

    // If true, the editor will prevent deletion of this element (protected/core elements)
    public bool CanBeDeleted { get; set; } = true;

    // Default state - now a full property instead of being stored in Properties dictionary
    [JsonInclude]
    public string DefaultState { get; set; } = "default";

    [JsonInclude]
    public HashSet<string> Aliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
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

        // Ensure a sane default state exists and has a type-appropriate SVG
        EnsureDefaultStateExists();
    }

    private void EnsureDefaultStateExists()
    {
        // Normalize and ensure default state
        if (string.IsNullOrWhiteSpace(DefaultState))
        {
            DefaultState = "default";
        }
        else
        {
            DefaultState = DefaultState.Trim();
        }

        // If the States dictionary does not contain an entry for this default state, add one with a type-specific SVG
        if (!States.ContainsKey(DefaultState))
        {
            States[DefaultState] = new GameElementState("", "default");
        }
    }

      [JsonIgnore]
    public bool IsVisible
    {
        get => !Flags.TryGetValue(FlagKeys.IsVisible, out var v) || v;
        set => Flags[FlagKeys.IsVisible] = value;
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
        
        // Migrate old DefaultState from Properties dictionary if it exists
        if (Properties.TryGetValue(PropertyKeys.DefaultState, out var oldDefaultState) && !string.IsNullOrWhiteSpace(oldDefaultState))
        {
            DefaultState = oldDefaultState.Trim();
            Properties.Remove(PropertyKeys.DefaultState); // Remove the old key
        }
        
        // Normalize DefaultState
        if (!string.IsNullOrWhiteSpace(DefaultState))
        {
            DefaultState = DefaultState.Trim();
        }

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
