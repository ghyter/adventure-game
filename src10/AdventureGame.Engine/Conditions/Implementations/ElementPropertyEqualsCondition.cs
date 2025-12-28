using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element's property equals a value.
/// </summary>
public sealed class ElementPropertyEqualsCondition : IConditionOperator
{
    public string Key => "element_property_equals";
    
    public string DisplayName => "Element Property Equals";
    
    public string Description => "Checks if an element's property value equals a specified value";
    
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
            Name = "propertyName",
            DisplayName = "Property Name",
            ParameterType = "propertyName",
            IsOptional = false,
            Description = "Name of the property"
        },
        new()
        {
            Name = "value",
            DisplayName = "Value",
            ParameterType = "string",
            IsOptional = false,
            Description = "Expected property value"
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
            !parameters.TryGetValue("propertyName", out var propName) ||
            !parameters.TryGetValue("value", out var expectedValue))
        {
            return false;
        }

        var element = ElementReferenceResolver.ResolveElement(elementId, round, session);
        if (element == null || !element.Properties.TryGetValue(propName, out var propValue))
        {
            return false;
        }

        var ignoreCase = !parameters.TryGetValue("ignoreCase", out var ic) || 
                        ic.Equals("true", StringComparison.OrdinalIgnoreCase);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(propValue, expectedValue, comparison);
    }
}
