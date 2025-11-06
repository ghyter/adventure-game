namespace AdventureGame.Engine.Models.Round;

public abstract class GameAction
{
    /// <summary>Canonical action name, e.g. "look", "use", "take".</summary>
    public string Name { get; set; } = "";

    /// <summary>Natural-language aliases, case-insensitive (e.g., "examine", "inspect").</summary>
    public HashSet<string> Aliases { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Human-readable description/tooling help.</summary>
    public string Description { get; set; } = "";

    /// <summary>Data-modeled condition tree. Evaluate against GameSession.</summary>
    public Condition? Conditions { get; set; }

    /// <summary>Effects to apply when the action succeeds.</summary>
    public List<GameEffect> Effects { get; } = [];
}

public sealed class Verb : GameAction
{
    // Each slot can hold one element id (or null)
    public string? Target1Id { get; set; }
    public string? Target2Id { get; set; }

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

// -----------------------------
// Triggers
// -----------------------------
public sealed class Trigger : GameAction
{
    public bool FireOnce { get; set; } = true;
    public bool Fired { get; set; } = false;
}