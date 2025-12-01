#nullable enable
using AdventureGame.Engine.Helpers;

namespace AdventureGame.Engine.Models;

/// <summary>
/// Signed 2D grid coordinate (origin-centered).
/// +X = East, -X = West
/// +Y = North (up)
/// -Y = South (down)
/// Levels are represented separately via a Level Element reference on Location.
/// </summary>
public readonly record struct GridPosition(int X, int Y)
{
    public static readonly GridPosition Origin = new(0, 0);
    public override string ToString() => $"({X},{Y})";
    public static GridPosition operator +(GridPosition a, GridPosition b) => new(a.X + b.X, a.Y + b.Y);
}

/// Grid configuration for the pack (cell size is in game units, not pixels).
public sealed class GridConfig
{
    public Dimensions CellSize { get; set; } = new(1, 1);
    public GridPosition Entry { get; set; } = GridPosition.Origin;
}

/// 8 compass directions + vertical.
public enum Direction { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Up, Down }

public static class GridNav
{
    public static GridPosition Delta(Direction dir) => dir switch
    {
        Direction.North => new(0, 1),
        Direction.NorthEast => new(1, 1),
        Direction.East => new(1, 0),
        Direction.SouthEast => new(1, -1),
        Direction.South => new(0, -1),
        Direction.SouthWest => new(-1, -1),
        Direction.West => new(-1, 0),
        Direction.NorthWest => new(-1, 1),
        // Up/Down are vertical and do not change X/Y; level transitions are handled via Level references
        Direction.Up => new(0, 0),
        Direction.Down => new(0, 0),
        _ => default
    };
}
