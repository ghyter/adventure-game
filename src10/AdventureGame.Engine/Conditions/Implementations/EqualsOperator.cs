using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition operator that checks if two values are equal.
/// Supports string, numeric, and boolean comparisons.
/// </summary>
public sealed class EqualsOperator : IConditionOperator
{
    public string Key => "equals";
    
    public string DisplayName => "Equals";
    
    public string Description => "Checks if two values are equal";
    
    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "value1",
            DisplayName = "First Value",
            ParameterType = "string",
            IsOptional = false,
            Description = "First value to compare"
        },
        new()
        {
            Name = "value2",
            DisplayName = "Second Value",
            ParameterType = "string",
            IsOptional = false,
            Description = "Second value to compare"
        },
        new()
        {
            Name = "ignoreCase",
            DisplayName = "Ignore Case",
            ParameterType = "boolean",
            IsOptional = true,
            Description = "Whether to ignore case in string comparison",
            DefaultValue = "true"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("value1", out var val1) || !parameters.TryGetValue("value2", out var val2))
        {
            return false;
        }

        var ignoreCase = !parameters.TryGetValue("ignoreCase", out var ic) || 
                        ic.Equals("true", StringComparison.OrdinalIgnoreCase);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(val1, val2, comparison);
    }
}
