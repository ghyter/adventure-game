#nullable enable
using AdventureGame.Engine.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;

/// <summary>
/// Signed 3D grid coordinate (origin-centered).
/// +X = East, -X = West
/// +Y = North (up)
/// -Y = South (down)
/// +Z = Up (vertical above)
/// -Z = Down (vertical below)
/// </summary>
public readonly record struct GridPosition(int X, int Y, int Z)
{
    public static readonly GridPosition Origin = new(0, 0, 0);
    public override string ToString() => $"({X},{Y},{Z})";
    public static GridPosition operator +(GridPosition a, GridPosition b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
}

/// Grid configuration for the pack (cell size is in game units, not pixels).
public sealed class GridConfig
{
    public Dimensions CellSize { get; set; } = new(1, 1, 1);
    public GridPosition Entry { get; set; } = GridPosition.Origin;
}

/// 8 compass directions + vertical.
public enum Direction { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest, Up, Down }

public static class GridNav
{
    public static GridPosition Delta(Direction dir) => dir switch
    {
        Direction.North => new(0, 1, 0),
        Direction.NorthEast => new(1, 1, 0),
        Direction.East => new(1, 0, 0),
        Direction.SouthEast => new(1, -1, 0),
        Direction.South => new(0, -1, 0),
        Direction.SouthWest => new(-1, -1, 0),
        Direction.West => new(-1, 0, 0),
        Direction.NorthWest => new(-1, 1, 0),
        Direction.Up => new(0, 0, 1),
        Direction.Down => new(0, 0, -1),
        _ => default
    };
}
