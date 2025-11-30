using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Represents a single condition that uses an operator and parameters.
/// This is the atomic unit of condition evaluation.
/// </summary>
public sealed class ConditionDefinition
{
    /// <summary>
    /// The key of the condition operator to use (e.g., "equals", "contains", "diceCheck")
    /// </summary>
    [JsonInclude]
    public string OperatorKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Parameters specific to the operator
    /// </summary>
    [JsonInclude]
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
