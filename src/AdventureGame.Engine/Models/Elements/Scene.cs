// ==============================
// AdventureGame.Engine/Models/Scene.cs
// ==============================
namespace AdventureGame.Engine.Models.Elements;

// --- Concrete elements ---
public sealed class Scene : GameElement
{
    private Dimensions _extent = new(1, 1, 1);

    public Dimensions ExtentInCells
    {
        get => _extent;
        set => _extent = new Dimensions(
            Math.Max(1, value.Rows),    // rows -> Y
            Math.Max(1, value.Columns), // columns -> X
            Math.Max(1, value.Levels)); // levels -> Z
    }



    public IEnumerable<GridPosition> OccupiedCells()
    {
        if (!Location.IsWorld || !Location.TryGetPosition(out var pos)) yield break;
        // Columns -> X axis, Rows -> Y axis, Levels -> Z axis
        for (var dx = 0; dx < ExtentInCells.Columns; dx++)
            for (var dy = 0; dy < ExtentInCells.Rows; dy++)
                for (var dz = 0; dz < ExtentInCells.Levels; dz++)
                    yield return new GridPosition(pos.X + dx, pos.Y + dy, pos.Z + dz);
    }
}
