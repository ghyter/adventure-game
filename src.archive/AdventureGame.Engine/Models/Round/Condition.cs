namespace AdventureGame.Engine.Models.Round;

// Models/Condition.cs
public sealed class Condition
{
    public string GameElementId { get; set; } = "";
    public string Rule { get; set; } = "";  // e.g. "HasState", "InLocation", etc.
    public string Comparison { get; set; } = "equals";
    public string Value { get; set; } = "";
}
