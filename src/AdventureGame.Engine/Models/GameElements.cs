// ==============================
// AdventureGame.Engine/Models/GameElements.cs
// ==============================
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

    [JsonInclude]
    public HashSet<string> Aliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, int> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, string?> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, bool> Flags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonInclude]
    public Dictionary<string, GameState> States { get; set; } = new(StringComparer.OrdinalIgnoreCase);

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
            States[ds] = new GameState("", svg);
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
        States[key] = new GameState(description ?? "", svg ?? "");
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

    public void SetStates(IEnumerable<KeyValuePair<string, GameState>> states, string defaultState)
    {
        States.Clear();
        foreach (var kv in states)
        {
            var key = (kv.Key ?? "").Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("State name cannot be empty.", nameof(states));
            States[key] = kv.Value ?? new GameState("", null);
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


    public abstract string ToSvg();
    public virtual string ToMapSvg() => ToSvg(); // fallback behavior

}

// --- Concrete elements ---
public sealed class Scene : GameElement
{
    private Dimensions _extent = new(1, 1, 1);

    public Dimensions ExtentInCells
    {
        get => _extent;
        set => _extent = new Dimensions(
            Math.Max(1, value.Length),
            Math.Max(1, value.Width),
            Math.Max(1, value.Height));
    }



    public IEnumerable<GridPosition> OccupiedCells()
    {
        if (!Location.IsWorld || !Location.TryGetPosition(out var pos)) yield break;
        for (var dx = 0; dx < ExtentInCells.Length; dx++)
            for (var dy = 0; dy < ExtentInCells.Width; dy++)
                for (var dz = 0; dz < ExtentInCells.Height; dz++)
                    yield return new GridPosition(pos.X + dx, pos.Y + dy, pos.Z + dz);
    }

    public override string ToSvg()
    {
        // Scene might have a decorative SVG background (optional)
        if (States.TryGetValue(DefaultState, out var s) && !string.IsNullOrWhiteSpace(s.Svg))
            return s.Svg!;

        return $"<rect width='100' height='100' fill='#1a1a1a' stroke='#333' />";
    }

    public override string ToMapSvg()
    {
        // Each Scene cell drawn as a rectangle on the map
        var color = IsVisible ? "#2e8b57" : "#666";
        var label = System.Security.SecurityElement.Escape(Name);
        var (x, y, z) = Location.TryGetPosition(out var p) ? (p.X, p.Y, p.Z) : (0, 0, 0);

        return $@"
            <g transform='translate({x * 40},{y * 40})'>
                <rect width='38' height='38' fill='{color}' stroke='black' rx='4' ry='4'/>
                <text x='19' y='24' font-size='10' text-anchor='middle' fill='white'>{label}</text>
            </g>";
    }
}

public sealed class Item : GameElement
{
    public Item()
    {
        Flags.TryAdd(FlagKeys.IsMovable, true);
    }

    [JsonIgnore]
    public bool IsMovable
    {
        get => Flags.TryGetValue(FlagKeys.IsMovable, out var v) && v;
        set => Flags[FlagKeys.IsMovable] = value;
    }

    public override void OnDeserialized()
    {
        base.OnDeserialized();
        Flags.TryAdd(FlagKeys.IsMovable, true);
    }

    public override void ValidateStatesOrThrow()
    {
        base.ValidateStatesOrThrow();
        Flags.TryAdd(FlagKeys.IsMovable, true);
    }

    public override string ToSvg()
    {
        // Use current state’s SVG (if available)
        if (States.TryGetValue(DefaultState, out var s) && !string.IsNullOrWhiteSpace(s.Svg))
            return s.Svg!;

        // Fallback default rendering
        var color = IsMovable ? "#ffcc00" : "#888";
        return $"<circle r='8' fill='{color}'><title>{Name}</title></circle>";
    }

    public override string ToMapSvg()
        => $"<text x='0' y='0' font-size='8' fill='#ccc'>{Name}</text>";

}

public sealed class Npc : GameElement {
    public override string ToSvg()
    {
        // Use current state’s SVG (if available)
        if (States.TryGetValue(DefaultState, out var s) && !string.IsNullOrWhiteSpace(s.Svg))
            return s.Svg!;

        // Fallback default rendering
        var color = "#ffcc00";
        return $"<circle r='8' fill='{color}'><title>{Name}</title></circle>";
    }

    public override string ToMapSvg()
        => $"<text x='0' y='0' font-size='8' fill='#ccc'>{Name}</text>";
}

public sealed class Player : GameElement {
    public override string ToSvg()
    {
        // Use current state’s SVG (if available)
        if (States.TryGetValue(DefaultState, out var s) && !string.IsNullOrWhiteSpace(s.Svg))
            return s.Svg!;

        // Fallback default rendering
        var color = "#ffcc00";
        return $"<circle r='8' fill='{color}'><title>{Name}</title></circle>";
    }

    public override string ToMapSvg()
        => $"<text x='0' y='0' font-size='8' fill='#ccc'>{Name}</text>";
}



public sealed class Exit : GameElement
{
    public HashSet<ElementId> Targets { get; } = new();
    public ExitMode Mode { get; set; } = ExitMode.Directional;
    public Direction? Direction { get; set; }
    public bool IsBidirectional { get; set; }

    public string? Label
    {
        get => Properties.TryGetValue("label", out var v) ? v : null;
        set => Properties["label"] = value;
    }
    
    public override string ToSvg()
    {
        // Use current state’s SVG (if available)
        if (States.TryGetValue(DefaultState, out var s) && !string.IsNullOrWhiteSpace(s.Svg))
            return s.Svg!;

        // Fallback default rendering
        var color = "#ffcc00";
        return $"<circle r='8' fill='{color}'><title>{Name}</title></circle>";
    }

    public override string ToMapSvg()
        => $"<text x='0' y='0' font-size='8' fill='#ccc'>{Name}</text>";

}
