// ==============================
// AdventureGame.Engine/Models/Scene.cs
// ==============================
namespace AdventureGame.Engine.Models;

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
