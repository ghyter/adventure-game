using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Represents a discoverable, metadata-driven condition operator.
/// Condition operators evaluate to true/false and are used in triggers, verb requirements, etc.
/// All condition operators must be registered in DI for catalog discovery.
/// </summary>
public interface IConditionOperator
{
    /// <summary>
    /// Unique identifier for this condition operator (e.g., "equals", "contains", "diceCheck")
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Human-readable name shown in the editor UI
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Detailed description of what this condition checks
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// List of parameters this condition requires/accepts
    /// </summary>
    IReadOnlyList<ConditionParameterDescriptor> Parameters { get; }
    
    /// <summary>
    /// Evaluates the condition with the given parameters.
    /// </summary>
    /// <param name="round">The current game round containing targets and state</param>
    /// <param name="session">The game session containing world state</param>
    /// <param name="parameters">Condition-specific parameters from the definition</param>
    /// <returns>True if the condition is met, false otherwise</returns>
    bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters);
}
