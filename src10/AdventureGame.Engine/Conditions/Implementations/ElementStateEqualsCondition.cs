using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element's current state equals a specific state name.
/// </summary>
public sealed class ElementStateEqualsCondition : IConditionOperator
{
    public string Key => "element_state_equals";
    
    public string DisplayName => "Element State Equals";
    
    public string Description => "Checks if an element's current state matches the specified state";
    
    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "element",
            DisplayName = "Element",
            ParameterType = "gameElement",
            IsOptional = false,
            Description = "The element to check"
        },
        new()
        {
            Name = "stateName",
            DisplayName = "State Name",
            ParameterType = "stateName",
            IsOptional = false,
            Description = "Expected state name"
        },
        new()
        {
            Name = "ignoreCase",
            DisplayName = "Ignore Case",
            ParameterType = "boolean",
            IsOptional = true,
            DefaultValue = "true",
            Description = "Whether to ignore case in comparison"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementId) || 
            !parameters.TryGetValue("stateName", out var expectedState))
        {
            return false;
        }

        var element = ElementReferenceResolver.ResolveElement(elementId, round, session);
        if (element == null) return false;

        var ignoreCase = !parameters.TryGetValue("ignoreCase", out var ic) || 
                        ic.Equals("true", StringComparison.OrdinalIgnoreCase);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(element.DefaultState, expectedState, comparison);
    }
}
