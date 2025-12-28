using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Effects.Implementations;

/// <summary>
/// Changes an element's state by setting its DefaultState property.
/// </summary>
public sealed class SetElementStateEffect : IEffectAction
{
    public string Key => "set_element_state";

    public string DisplayName => "Set Element State";

    public string Description => "Changes an element's current state";

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
            Name = "stateName",
            DisplayName = "State Name",
            ParameterType = "stateName",
            IsOptional = false,
            Description = "The state to set"
        }
    ];

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementRef) ||
            !parameters.TryGetValue("stateName", out var stateName))
        {
            throw new InvalidOperationException("SetElementStateEffect requires 'element' and 'stateName' parameters");
        }

        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null)
        {
            throw new InvalidOperationException($"Element '{elementRef}' not found");
        }

        if (!element.States.ContainsKey(stateName))
        {
            throw new InvalidOperationException($"State '{stateName}' does not exist on element '{element.Name}'");
        }

        element.DefaultState = stateName;
        return Task.CompletedTask;
    }
}
