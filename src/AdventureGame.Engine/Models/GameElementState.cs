// ==============================
// AdventureGame.Engine/Models/GameState.cs
// ==============================
#nullable enable
namespace AdventureGame.Engine.Models;

public sealed class GameElementState
{
    public GameElementState(string description, string? svg = null)
    {
        Description = description;
        Svg = svg;
    }
    public string Description { get; set; } = "";
    public string? Svg { get; set; } = null;
}