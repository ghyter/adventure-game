// ==============================
// AdventureGame.Engine/Extensions/GamePackMapExtensions.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Extensions;

/// <summary>
/// Provides helper methods for spatial lookups and neighbor traversal on a GamePack.
/// These are stateless utilities that do not modify or cache inside GamePack.
/// </summary>
public static class GamePackMapExtensions
{
    public static bool TryGetSceneAt(this GamePack pack, GridPosition cell, out Scene? scene)
        => pack.Elements.TryGetSceneAt(cell, out scene);

    public static bool TryGetSceneAt(this GameSession session, GridPosition cell, out Scene? scene)
        => session.Elements.TryGetSceneAt(cell, out scene);

    /// <summary>
    /// Tries to find the Scene occupying a given grid position.
    /// </summary>
    public static bool TryGetSceneAt(this IEnumerable<GameElement> elements, GridPosition cell, out Scene? scene)
    {
        scene = elements.OfType<Scene>()
            .FirstOrDefault(s => s.OccupiedCells().Contains(cell));
        return scene is not null;
    }



    public static IEnumerable<(Direction Dir, GridPosition Pos, Scene? Scene)>
        GetNeighbors(this GamePack pack, GridPosition cell)
        => pack.Elements.GetNeighbors(cell);

    public static IEnumerable<(Direction Dir, GridPosition Pos, Scene? Scene)>
        GetNeighbors(this GameSession session, GridPosition cell)
        => session.Elements.GetNeighbors(cell);


    /// <summary>
    /// Gets the neighboring scenes around the given scene's occupied area.
    /// Respects the scene's extent so neighbors are beyond the current scene's footprint,
    /// and never returns the same scene.
    /// </summary>
    public static IEnumerable<(Direction Dir, GridPosition Pos, Scene? Scene)>
        GetNeighbors(this IEnumerable<GameElement> elements, GridPosition cell)
    {
        // Determine which scene occupies the provided cell
        if (!elements.TryGetSceneAt(cell, out var self) || self == null)
            yield break;

        var extent = self.ExtentInCells;
        if (extent.Columns <= 0 || extent.Rows <= 0)
            yield break;

        // Half-extents help calculate how far to step past the scene footprint
        var halfWidth = extent.Width / 2;   // width == rows
        var halfLength = extent.Length / 2; // length == columns

        // Determine this scene's level id (may be null)
        elements.TryGetSceneAt(cell, out _); // noop, keep pattern
        var selfLevel = self.Location.TryGetLevel(out var lid) ? lid : (ElementId?)null;

        foreach (var dir in new[]
        {
        Direction.NorthWest, Direction.North, Direction.NorthEast,
        Direction.West,                      Direction.East,
        Direction.SouthWest, Direction.South, Direction.SouthEast,
        Direction.Up, Direction.Down
    })
        {
            if (dir == Direction.Up || dir == Direction.Down)
            {
                // Vertical neighbors: scenes at the same X,Y but different LevelId
                var candidates = elements.OfType<Scene>()
                    .Where(s => s.Location.TryGetPosition(out var sp) && sp.X == cell.X && sp.Y == cell.Y)
                    .Where(s => s.Id != self.Id)
                    .ToList();

                foreach (var cand in candidates)
                {
                    // include vertical neighbor regardless of its level id
                    yield return (dir, cand.Location.TryGetPosition(out var cp) ? cp : GridPosition.Origin, cand);
                }

                continue;
            }

            var delta = GridNav.Delta(dir);

            // Scale offsets based on scene size: step beyond its occupied edge
            var stepX = delta.X != 0 ? halfWidth + 1 : 0;
            var stepY = delta.Y != 0 ? halfLength + 1 : 0;

            var pos = new GridPosition(
                cell.X + delta.X * stepX,
                cell.Y + delta.Y * stepY
            );

            elements.TryGetSceneAt(pos, out var neighbor);

            // Ensure neighbor is on the same level as self (if self has a level set)
            if (neighbor is not null && selfLevel is not null)
            {
                if (!neighbor.Location.TryGetLevel(out var nlevel) || nlevel != selfLevel)
                {
                    neighbor = null; // ignore cross-level neighbor for horizontal adjacency
                }
            }

            // ✅ Skip self (covers overlapping cells for large extents)
            if (neighbor == self)
                continue;

            yield return (dir, pos, neighbor);
        }
    }

}
