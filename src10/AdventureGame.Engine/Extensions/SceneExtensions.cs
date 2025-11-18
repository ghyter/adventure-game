// ==============================
// AdventureGame.Engine/Models/SceneExtensions.cs
// ==============================
using AdventureGame.Engine.Models.Elements;

namespace AdventureGame.Engine.Extensions;

public static class SceneExtensions
{

    /// <summary>
    /// Computes the minimum and maximum X/Y grid bounds that encompass all scenes,
    /// factoring in each scene’s ExtentInCells.
    /// Returns (-1..1, -1..1) when no scenes are present.
    /// Optionally expands by one cell on every side when includeBuffer == true.
    /// </summary>
    public static (int MinX, int MaxX, int MinY, int MaxY)
        GetBounds(this IEnumerable<Scene> scenes, bool includeBuffer = true)
    {
        if (scenes is null || !scenes.Any())
            return (-1, 1, -1, 1);

        var bounds = scenes
            .Where(s => s.Position is not null)
            .Select(s => new
            {
                MinX = s.Position!.Value.X,
                MaxX = s.Position!.Value.X + Math.Max(1, s.ExtentInCells.Columns) - 1,
                MinY = s.Position!.Value.Y,
                MaxY = s.Position!.Value.Y + Math.Max(1, s.ExtentInCells.Rows) - 1
            })
            .ToList();

        if (bounds.Count == 0)
            return (-1, 1, -1, 1);

        int minX = bounds.Min(b => b.MinX);
        int maxX = bounds.Max(b => b.MaxX);
        int minY = bounds.Min(b => b.MinY);
        int maxY = bounds.Max(b => b.MaxY);

        if (includeBuffer)
        {
            minX -= 1; maxX += 1;
            minY -= 1; maxY += 1;
        }

        return (minX, maxX, minY, maxY);
    }
}
