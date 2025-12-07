using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Effects.Implementations;

/// <summary>
/// Sets an element's name.
/// </summary>
public sealed class SetElementNameEffect : IEffectAction
{
    public string Key => "set_element_name";

    public string DisplayName => "Set Element Name";

    public string Description => "Changes an element's name";

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
            Name = "name",
            DisplayName = "New Name",
            ParameterType = "string",
            IsOptional = false,
            Description = "The new name for the element"
        }
    ];

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementRef) ||
            !parameters.TryGetValue("name", out var newName))
        {
            throw new InvalidOperationException("SetElementNameEffect requires 'element' and 'name' parameters");
        }

        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null)
        {
            throw new InvalidOperationException($"Element '{elementRef}' not found");
        }

        element.Name = newName;
        return Task.CompletedTask;
    }
}
