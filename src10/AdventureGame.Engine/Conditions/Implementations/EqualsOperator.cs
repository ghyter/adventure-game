using AdventureGame.Engine.Models.Actions;
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
    
    public IReadOnlyList<ConditionParameterDescriptor> Parameters { get; } = new List<ConditionParameterDescriptor>
    {
        new()
        {
            Name = "value1",
            Kind = ConditionParameterKind.Value,
            IsRequired = true,
            Description = "First value to compare"
        },
        new()
        {
            Name = "value2",
            Kind = ConditionParameterKind.Value,
            IsRequired = true,
            Description = "Second value to compare"
        },
        new()
        {
            Name = "ignoreCase",
            Kind = ConditionParameterKind.Boolean,
            IsRequired = false,
            Description = "Whether to ignore case in string comparison",
            DefaultValue = "true"
        }
    };
    
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
