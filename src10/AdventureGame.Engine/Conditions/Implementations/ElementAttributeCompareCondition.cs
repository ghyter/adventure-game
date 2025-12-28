using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element's attribute value meets a numeric comparison.
/// </summary>
public sealed class ElementAttributeCompareCondition : IConditionOperator
{
    public string Key => "element_attribute_compare";
    
    public string DisplayName => "Element Attribute Compare";
    
    public string Description => "Compares an element's attribute value against a number";
    
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
            Name = "attributeName",
            DisplayName = "Attribute Name",
            ParameterType = "attributeName",
            IsOptional = false,
            Description = "Name of the attribute"
        },
        new()
        {
            Name = "operator",
            DisplayName = "Operator",
            ParameterType = "comparisonOperator",
            IsOptional = false,
            Description = "Comparison operator"
        },
        new()
        {
            Name = "value",
            DisplayName = "Value",
            ParameterType = "number",
            IsOptional = false,
            Description = "Value to compare against"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementId) || 
            !parameters.TryGetValue("attributeName", out var attrName) ||
            !parameters.TryGetValue("operator", out var op) ||
            !parameters.TryGetValue("value", out var valueStr))
        {
            return false;
        }

        var element = ElementReferenceResolver.ResolveElement(elementId, round, session);
        if (element == null || !element.Attributes.TryGetValue(attrName, out var attrValue))
        {
            return false;
        }

        if (!int.TryParse(valueStr, out var compareValue))
        {
            return false;
        }

        return op.ToLowerInvariant() switch
        {
            "equals" or "eq" => attrValue == compareValue,
            "less_than" or "lt" => attrValue < compareValue,
            "greater_than" or "gt" => attrValue > compareValue,
            "less_equal" or "le" => attrValue <= compareValue,
            "greater_equal" or "ge" => attrValue >= compareValue,
            _ => false
        };
    }
}
