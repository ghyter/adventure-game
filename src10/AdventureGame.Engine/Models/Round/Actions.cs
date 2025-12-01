using AdventureGame.Engine.Conditions;
using AdventureGame.Engine.Effects;

namespace AdventureGame.Engine.Models.Round;

public abstract class GameAction
{
    /// <summary>Canonical action name, e.g. "look", "use", "take".</summary>
    public string? Name { get; set; } = string.Empty;

    /// <summary>Natural-language aliases, case-insensitive (e.g., "examine", "inspect").</summary>
    public HashSet<string> Aliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Human-readable description/tooling help.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Data-modeled condition definition. Evaluate against GameSession.</summary>
    public ConditionDefinition? ConditionDef { get; set; }

    /// <summary>Effects to apply when the action succeeds.</summary>
    public List<EffectDefinition> Effects { get; } = [];
}
