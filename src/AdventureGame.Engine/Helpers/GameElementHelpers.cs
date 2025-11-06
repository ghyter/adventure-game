using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGame.Engine.Helpers;



// --- Shared constants ---
public static class FlagKeys
{
    public const string IsVisible = "isVisible";
    public const string IsMovable = "isMovable";
}
public static class PropertyKeys
{
    public const string DefaultState = "defaultState";
    public const string DefaultAlias = "defaultAlias";
}

// --- Supporting types ---
public sealed class Dimensions(int rows, int columns, int levels)
{
    // Backing properties kept for compatibility
    public int Length { get; set; } = columns;
    public int Width { get; set; } = rows;
    public int Height { get; set; } = levels;

    // New semantic aliases: Rows (Y), Columns (X), Levels (Z)
    // Rows correspond to the grid Y dimension (number of rows)
    public int Rows { get => Width; set => Width = value; }
    // Columns correspond to the grid X dimension (number of columns)
    public int Columns { get => Length; set => Length = value; }
    // Levels correspond to the grid Z dimension (number of levels)
    public int Levels { get => Height; set => Height = value; }

    public static Dimensions Default => new(1, 1, 1);
}

