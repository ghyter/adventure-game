namespace AdventureGame.Engine.Models.Actions;

public sealed class ModifierSource
{
    public TargetSelector? Source { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Modifier { get; set; }
}
