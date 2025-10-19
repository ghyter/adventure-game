// ==============================
// AdventureGame.Engine/Extensions/GamePackMapExtensions.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Extensions;

/// <summary>
/// Provides helper methods for spatial lookups and neighbor traversal on a GamePack.
/// These are stateless utilities that do not modify or cache inside GamePack.
/// </summary>
public static class GamePackMapExtensions
{
    /// <summary>
    /// Tries to find the Scene occupying a given grid position.
    /// </summary>
    public static bool TryGetSceneAt(this GamePack pack, GridPosition cell, out Scene? scene)
    {
        scene = pack.Elements.OfType<Scene>()
            .FirstOrDefault(s => s.OccupiedCells().Contains(cell));
        return scene is not null;
    }

    /// <summary>
    /// Gets the neighboring scenes around a cell in all directions.
    /// </summary>
    public static IEnumerable<(Direction Dir, GridPosition Pos, Scene? Scene)>
        GetNeighbors(this GamePack pack, GridPosition cell)
    {
        foreach (var dir in new[]
        {
            Direction.NorthWest, Direction.North, Direction.NorthEast,
            Direction.West,                      Direction.East,
            Direction.SouthWest, Direction.South, Direction.SouthEast,
            Direction.Up, Direction.Down
        })
        {
            var delta = GridNav.Delta(dir);
            var pos = new GridPosition(cell.X + delta.X, cell.Y + delta.Y, cell.Z + delta.Z);
            pack.TryGetSceneAt(pos, out var s);
            yield return (dir, pos, s);
        }
    }
}
