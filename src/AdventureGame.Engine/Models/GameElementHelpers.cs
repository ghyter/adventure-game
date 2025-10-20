using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventureGame.Engine.Models;


public enum ExitMode { Directional, Custom, Portal }
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
public readonly record struct Dimensions(int Length, int Width, int Height);

