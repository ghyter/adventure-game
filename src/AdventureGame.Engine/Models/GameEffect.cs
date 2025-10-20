namespace AdventureGame.Engine.Models;

// Models/GameEffect.cs
public sealed class GameEffect
{
    public string TargetId { get; set; } = "";
    public string Action { get; set; } = ""; // e.g. "ChangeState", "MoveTo", etc.
    public string Value { get; set; } = "";
}
