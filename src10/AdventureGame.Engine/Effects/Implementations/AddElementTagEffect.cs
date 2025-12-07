using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Effects.Implementations;

/// <summary>
/// Adds a tag to an element's tag collection.
/// </summary>
public sealed class AddElementTagEffect : IEffectAction
{
    public string Key => "add_element_tag";

    public string DisplayName => "Add Element Tag";

    public string Description => "Adds a tag to an element";

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
            Name = "tag",
            DisplayName = "Tag",
            ParameterType = "tag",
            IsOptional = false,
            Description = "The tag to add"
        }
    ];

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementRef) ||
            !parameters.TryGetValue("tag", out var tag))
        {
            throw new InvalidOperationException("AddElementTagEffect requires 'element' and 'tag' parameters");
        }

        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null)
        {
            throw new InvalidOperationException($"Element '{elementRef}' not found");
        }

        element.Tags.Add(tag);
        return Task.CompletedTask;
    }
}
