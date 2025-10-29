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
    public int Length { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public Dimensions(int length, int width, int height)
    {
        Length = length;
        Width = width;
        Height = height;
    }

    public static Dimensions Default => new Dimensions(1, 1, 1);
}

