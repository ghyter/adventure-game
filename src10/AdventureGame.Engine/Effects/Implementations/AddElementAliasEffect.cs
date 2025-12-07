using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Effects.Implementations;

/// <summary>
/// Adds an alias to an element's alias collection.
/// </summary>
public sealed class AddElementAliasEffect : IEffectAction
{
    public string Key => "add_element_alias";

    public string DisplayName => "Add Element Alias";

    public string Description => "Adds an alias to an element";

    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "element",
            DisplayName = "Element",
            ParameterType = "gameElement",
            IsOptional = false,
            Description = "The element to modify"
        },
        new()
        {
            Name = "alias",
            DisplayName = "Alias",
            ParameterType = "alias",
            IsOptional = false,
            Description = "The alias to add"
        }
    ];

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementRef) ||
            !parameters.TryGetValue("alias", out var alias))
        {
            throw new InvalidOperationException("AddElementAliasEffect requires 'element' and 'alias' parameters");
        }

        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null)
        {
            throw new InvalidOperationException($"Element '{elementRef}' not found");
        }

        element.Aliases.Add(alias);
        return Task.CompletedTask;
    }
}
