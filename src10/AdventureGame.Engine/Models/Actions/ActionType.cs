namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Distinguishes between player-initiated verbs and automatic triggers.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Player-initiated action bound to a verb phrase and targets
    /// </summary>
    Verb,
    
    /// <summary>
    /// Automatic action that executes at the end of a round
    /// </summary>
    Trigger
}
