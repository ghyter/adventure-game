#nullable enable

namespace AdventureGame.Engine.Models.Actions;

public sealed class DifficultyCheck
{
    public int Difficulty { get; set; } = 0;
    public List<ModifierSource> Modifiers { get; set; } = [];
}

public sealed class ModifierSource
{
    public TargetSelector? Source { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Modifier { get; set; }
}
