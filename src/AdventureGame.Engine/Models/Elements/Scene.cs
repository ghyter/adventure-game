// ==============================
// AdventureGame.Engine/Models/Scene.cs
// ==============================
using AdventureGame.Engine.Helpers;

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
        // Columns -> X axis, Rows -> Y axis
        for (var dx = 0; dx < ExtentInCells.Columns; dx++)
            for (var dy = 0; dy < ExtentInCells.Rows; dy++)
                    yield return new GridPosition(pos.X + dx, pos.Y + dy);
    }
}
