namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Defines how conditions in a group combine logically.
/// </summary>
public enum GroupOperator
{
    /// <summary>
    /// All conditions must be true (AND logic)
    /// </summary>
    All,
    
    /// <summary>
    /// At least one condition must be true (OR logic)
    /// </summary>
    Any
}
