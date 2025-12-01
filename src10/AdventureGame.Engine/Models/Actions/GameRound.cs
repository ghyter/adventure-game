#nullable enable
using AdventureGame.Engine.Effects;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Round;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Represents a single executed round or user command.
/// Extended to support the new effect/condition system with structured targets and logs.
/// </summary>
public sealed class GameRound
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string RawInput { get; set; } = string.Empty;
    public string? ParsedVerb { get; set; }
    public List<TargetSelector> ParsedTargets { get; set; } = [];

    public string? MatchedVerbName { get; set; }

    public bool ConditionsMet { get; set; }
    public bool Success { get; set; }

    public int DieRoll { get; set; }
    public int TotalModifiers { get; set; }
    public int TotalRoll => DieRoll + TotalModifiers;

    public List<AppliedEffect> EffectsApplied { get; set; } = [];

    public string OutputText { get; set; } = string.Empty;

    // ---- NEW: Effect/Condition System Extensions ----

    /// <summary>
    /// Primary target element for this round (from parsed targets or context)
    /// </summary>
    public GameElement? Target1 { get; set; }

    /// <summary>
    /// Secondary target element for this round (for two-target verbs/actions)
    /// </summary>
    public GameElement? Target2 { get; set; }

    /// <summary>
    /// Structured output log for displaying to the player
    /// </summary>
    public List<string> Output { get; set; } = [];

    /// <summary>
    /// Debug/diagnostic log for dice rolls and internal calculations
    /// </summary>
    public List<string> DebugLog { get; set; } = [];

    /// <summary>
    /// Pending state changes to commit after round completes
    /// Key-value pairs for round-scoped temporary values
    /// </summary>
    private readonly Dictionary<string, string> _pendingChanges = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Sets a value in the round's pending changes dictionary
    /// </summary>
    public void SetValue(string key, string value)
    {
        _pendingChanges[key] = value;
    }

    /// <summary>
    /// Gets a value from the round's pending changes dictionary
    /// </summary>
    public string? GetValue(string key)
    {
        return _pendingChanges.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Gets all pending changes for this round
    /// </summary>
    public IReadOnlyDictionary<string, string> PendingChanges => _pendingChanges;
}

public sealed class AppliedEffect
{
    public EffectDefinition Effect { get; set; } = new();
    public int Roll { get; set; }
    public bool Success { get; set; }
}
