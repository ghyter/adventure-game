// ==============================
// AdventureGame.Engine/Validation/GamePackValidator.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;

namespace AdventureGame.Engine.Validation;

/// <summary>
/// Performs structural validation of a GamePack to ensure integrity before use.
/// This is the default validator; specialized validators can subclass or replace it.
/// </summary>
public sealed class GamePackValidator(bool requireAtLeastOnePlayer = true) : IGamePackValidator
{
    private readonly bool _requirePlayer = requireAtLeastOnePlayer;

    /// <summary>
    /// Runs a complete validation pass and throws an exception if invalid.
    /// </summary>
    public void Validate(GamePack pack)
    {
        if (string.IsNullOrWhiteSpace(pack.Name))
            throw new InvalidOperationException("GamePack.Name is required.");

        if (pack.Grid.CellSize.Length < 1 ||
            pack.Grid.CellSize.Width < 1 
            )
            throw new InvalidOperationException("Grid.CellSize must be ≥ 1 in every axis.");

        // Unique IDs
        if (pack.Elements.Count != pack.Elements.Select(e => e.Id).Distinct().Count())
            throw new InvalidOperationException("Duplicate ElementId detected.");

        // Player requirement
        if (_requirePlayer && !pack.Elements.OfType<Player>().Any())
            throw new InvalidOperationException("At least one Player element is required.");

        // Basic scene overlap check
        var occupied = new Dictionary<GridPosition, Scene>();
        foreach (var s in pack.Elements.OfType<Scene>())
        {
            foreach (var c in s.OccupiedCells())
            {
                if (occupied.TryGetValue(c, out var other))
                    throw new InvalidOperationException(
                        $"Scene '{s.Name}' overlaps scene '{other.Name}' at {c}.");
                occupied[c] = s;
            }
        }
    }
}
