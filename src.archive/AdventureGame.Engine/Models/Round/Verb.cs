namespace AdventureGame.Engine.Models.Round;

using AdventureGame.Engine.Models.Actions;

public sealed class Verb : GameAction
{
    // Each slot can hold one element id (or null)
    public string? Target1Id { get; set; }
    public string? Target2Id { get; set; }

    // Structured authoring additions (non-breaking)
    public ConditionGroup? Preconditions { get; set; }
    public DifficultyCheck DifficultyCheck { get; set; } = new();
    public EffectGroup? StructuredEffects { get; set; }
    public List<EffectRange>? EffectsByRange { get; set; }
    public string? SuccessMessage { get; set; }
    public string? FailureMessage { get; set; }

    // Simple rule helpers
    public bool HasTarget1 => !string.IsNullOrEmpty(Target1Id);
    public bool HasTarget2 => !string.IsNullOrEmpty(Target2Id);

    // Rough scoring heuristic: prefer verbs with fewer explicit targets
    public int MatchScore()
    {
        if (!HasTarget1 && !HasTarget2) return 1; // 0-target verbs like "look"
        if (HasTarget1 && !HasTarget2) return 2;  // 1-target verbs like "take apple"
        return 3;                                 // 2-target verbs like "use key door"
    }
}
