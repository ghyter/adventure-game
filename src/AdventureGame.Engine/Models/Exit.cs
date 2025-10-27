// ==============================
// AdventureGame.Engine/Models/Exit.cs
// ==============================
namespace AdventureGame.Engine.Models;

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
