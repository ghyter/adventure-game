using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGame.Engine.Models;



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
public sealed class Dimensions
{
    // Backing properties kept for compatibility
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    // New semantic aliases: Rows (Y), Columns (X), Levels (Z)
    // Rows correspond to the grid Y dimension (number of rows)
    public int Rows { get => Width; set => Width = value; }
    // Columns correspond to the grid X dimension (number of columns)
    public int Columns { get => Length; set => Length = value; }
    // Levels correspond to the grid Z dimension (number of levels)
    public int Levels { get => Height; set => Height = value; }

    // Constructor now documented as (rows, columns, levels)
    public Dimensions(int rows, int columns, int levels)
    {
        Length = columns;
        Width = rows;
        Height = levels;
    }

    public static Dimensions Default => new Dimensions(1, 1, 1);
}

