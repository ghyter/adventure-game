using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element has a specific alias.
/// </summary>
public sealed class ElementHasAliasCondition : IConditionOperator
{
    public string Key => "element_has_alias";
    
    public string DisplayName => "Element Has Alias";
    
    public string Description => "Checks if an element has a specific alias";
    
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
            Name = "alias",
            DisplayName = "Alias",
            ParameterType = "alias",
            IsOptional = false,
            Description = "The alias to check for"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementRef) || 
            !parameters.TryGetValue("alias", out var alias))
        {
            return false;
        }

        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null) return false;

        return element.Aliases.Contains(alias);
    }
}
