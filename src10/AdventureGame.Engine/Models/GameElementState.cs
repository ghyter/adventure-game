// ==============================
// AdventureGame.Engine/Models/GameState.cs
// ==============================
#nullable enable
namespace AdventureGame.Engine.Models;

public sealed class GameElementState(string description, string? svg = null)
{
    public string Description { get; set; } = description;
    public string? Svg { get; set; } = svg;
}