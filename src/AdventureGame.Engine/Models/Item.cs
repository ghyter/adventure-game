// ==============================
// AdventureGame.Engine/Models/Item.cs
// ==============================
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;

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
