using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Conditions.Implementations;

/// <summary>
/// Condition that checks if an element has a specific tag.
/// </summary>
public sealed class ElementHasTagCondition : IConditionOperator
{
    public string Key => "element_has_tag";
    
    public string DisplayName => "Element Has Tag";
    
    public string Description => "Checks if an element has a specific tag";
    
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
            Name = "tag",
            DisplayName = "Tag",
            ParameterType = "tag",
            IsOptional = false,
            Description = "The tag to check for"
        }
    ];
    
    public bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementId) || 
            !parameters.TryGetValue("tag", out var tag))
        {
            return false;
        }

        var element = ElementReferenceResolver.ResolveElement(elementId, round, session);
        if (element == null) return false;

        return element.Tags.Contains(tag);
    }
}
