using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element's Name matches a specified value.
/// </summary>
public sealed class ElementNameEqualsCondition : IConditionOperator
{
    public string Key => "element_name_equals";
    
    public string DisplayName => "Element Name Equals";
    
    public string Description => "Checks if an element's name matches the specified value";
    
    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "element",
            DisplayName = "Element",
            ParameterType = "gameElement",
            IsOptional = false,
            Description = "The element to check (name, alias, or special variable: target, target1, target2, currentScene, currentPlayer)"
        },
        new()
        {
            Name = "name",
            DisplayName = "Name",
            ParameterType = "string",
            IsOptional = false,
            Description = "Expected name value"
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
        if (!parameters.TryGetValue("element", out var elementRef) || 
            !parameters.TryGetValue("name", out var expectedName))
        {
            return false;
        }

        // Resolve element reference to actual element
        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null) return false;

        var ignoreCase = !parameters.TryGetValue("ignoreCase", out var ic) || 
                        ic.Equals("true", StringComparison.OrdinalIgnoreCase);

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return string.Equals(element.Name, expectedName, comparison);
    }
}
