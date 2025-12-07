using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element's flag is set to a specific boolean value.
/// </summary>
public sealed class ElementFlagIsCondition : IConditionOperator
{
    public string Key => "element_flag_is";
    
    public string DisplayName => "Element Flag Is";
    
    public string Description => "Checks if an element's flag is set to true or false";
    
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
            Name = "flagName",
            DisplayName = "Flag Name",
            ParameterType = "flagName",
            IsOptional = false,
            Description = "Name of the flag"
        },
        new()
        {
            Name = "value",
            DisplayName = "Expected Value",
            ParameterType = "boolean",
            IsOptional = false,
            DefaultValue = "true",
            Description = "Expected flag value (true/false)"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementId) || 
            !parameters.TryGetValue("flagName", out var flagName) ||
            !parameters.TryGetValue("value", out var expectedValueStr))
        {
            return false;
        }

        var element = ElementReferenceResolver.ResolveElement(elementId, round, session);
        if (element == null || !element.Flags.TryGetValue(flagName, out var flagValue))
        {
            return false;
        }

        var expectedValue = expectedValueStr.Equals("true", StringComparison.OrdinalIgnoreCase);
        return flagValue == expectedValue;
    }
}
