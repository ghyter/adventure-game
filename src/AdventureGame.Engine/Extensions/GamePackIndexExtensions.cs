// ==============================
// AdventureGame.Engine/Extensions/GamePackIndexExtensions.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Extensions;

/// <summary>
/// Provides helper methods for building and caching lookup dictionaries
/// from a GamePack. These are not stored within the pack.
/// </summary>
public static class GamePackIndexExtensions
{
    /// <summary>
    /// Builds a dictionary of all elements keyed by ElementId.
    /// </summary>
    public static IReadOnlyDictionary<ElementId, GameElement> BuildIdIndex(this GamePack pack)
        => pack.Elements.ToDictionary(e => e.Id, e => e);

    /// <summary>
    /// Builds a dictionary mapping each grid position to the Scene occupying it.
    /// </summary>
    public static IReadOnlyDictionary<GridPosition, Scene> BuildSceneIndex(this GamePack pack)
    {
        var map = new Dictionary<GridPosition, Scene>();
        foreach (var scene in pack.Elements.OfType<Scene>())
        {
            foreach (var cell in scene.OccupiedCells())
            {
                if (map.ContainsKey(cell))
                    throw new InvalidOperationException(
                        $"Overlapping scenes at {cell}: {map[cell].Name} and {scene.Name}");
                map[cell] = scene;
            }
        }
        return map;
    }

    /// <summary>
    /// Convenience helper: tries to resolve an element by ID.
    /// </summary>
    public static bool TryGetElement(this GamePack pack, ElementId id, out GameElement? element)
    {
        var index = pack.BuildIdIndex();
        return index.TryGetValue(id, out element);
    }
}
