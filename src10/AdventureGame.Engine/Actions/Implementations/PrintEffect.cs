using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Actions.Implementations;

/// <summary>
/// Effect action that prints a message to the game output log.
/// Useful for debugging and providing player feedback.
/// </summary>
public sealed class PrintEffect : IEffectAction
{
    public string Key => "print";
    
    public string DisplayName => "Print Message";
    
    public string Description => "Outputs a message to the game log";
    
    public IReadOnlyList<EffectParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "message",
            Kind = EffectParameterKind.Value,
            IsRequired = true,
            Description = "The message text to display"
        }
    ];
    
    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("message", out var message) && !string.IsNullOrWhiteSpace(message))
        {
            round.Output.Add(message);
        }
        
        return Task.CompletedTask;
    }
}
