#nullable enable
using AdventureGame.Engine.Models.Round;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>Represents a single executed round or user command.</summary>
public sealed class GameRound
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string RawInput { get; set; } = string.Empty;
    public string? ParsedVerb { get; set; }
    public List<TargetSelector> ParsedTargets { get; set; } = new();

    public string? MatchedVerbName { get; set; }

    public bool ConditionsMet { get; set; }
    public bool Success { get; set; }

    public int DieRoll { get; set; }
    public int TotalModifiers { get; set; }
    public int TotalRoll => DieRoll + TotalModifiers;

    public List<AppliedEffect> EffectsApplied { get; set; } = new();

    public string OutputText { get; set; } = string.Empty;
}

public sealed class AppliedEffect
{
    public GameEffect Effect { get; set; } = new();
    public int Roll { get; set; }
    public bool Success { get; set; }
}
