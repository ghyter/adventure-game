using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Extensions;

public static class DirectionExtensions
{
    // Short label used by UI (N, NE, E, SE, S, SW, W, NW, UP, DOWN)
    public static string ToAbbreviation(this Direction d) => d switch
    {
        Direction.North => "N",
        Direction.NorthEast => "NE",
        Direction.East => "E",
        Direction.SouthEast => "SE",
        Direction.South => "S",
        Direction.SouthWest => "SW",
        Direction.West => "W",
        Direction.NorthWest => "NW",
        Direction.Up => "UP",
        Direction.Down => "DOWN",
        _ => string.Empty
    };

    public static bool TryParseAbbreviation(string s, out Direction dir)
    {
        dir = default;
        if (string.IsNullOrWhiteSpace(s)) return false;
        return s.Trim().ToUpperInvariant() switch
        {
            "N" => (dir = Direction.North) != default,
            "NE" => (dir = Direction.NorthEast) != default,
            "E" => (dir = Direction.East) != default,
            "SE" => (dir = Direction.SouthEast) != default,
            "S" => (dir = Direction.South) != default,
            "SW" => (dir = Direction.SouthWest) != default,
            "W" => (dir = Direction.West) != default,
            "NW" => (dir = Direction.NorthWest) != default,
            "UP" => (dir = Direction.Up) != default,
            "DOWN" => (dir = Direction.Down) != default,
            _ => false
        };
    }

    // Compass rotation angle used by Compass component
    public static double Angle(this Direction d) => d switch
    {
        Direction.North => 0,
        Direction.NorthEast => 45,
        Direction.East => 90,
        Direction.SouthEast => 135,
        Direction.South => 180,
        Direction.SouthWest => 225,
        Direction.West => 270,
        Direction.NorthWest => 315,
        _ => 0
    };

    // Returns GridNav.Delta with optional Y inversion if your UI coordinate system differs.
    // Keep coordinate-conversion logic here so callers don't need to invert Y themselves.
    public static GridPosition Delta(this Direction d, bool invertY = false)
    {
        var baseDelta = GridNav.Delta(d);
        if (!invertY) return baseDelta;
        return new GridPosition(baseDelta.X, -baseDelta.Y, baseDelta.Z);
    }

    public static bool IsVertical(this Direction d) => d == Direction.Up || d == Direction.Down;
}