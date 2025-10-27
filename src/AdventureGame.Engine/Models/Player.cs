// ==============================
// AdventureGame.Engine/Models/Player.cs
// ==============================
namespace AdventureGame.Engine.Models;

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
