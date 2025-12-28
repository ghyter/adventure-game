using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Effects.Implementations;

/// <summary>
/// Moves the player through an exit to the target scene.
/// Finds the exit's target exit, then moves the player to that exit's parent scene.
/// </summary>
public sealed class MovePlayerThroughExitEffect : IEffectAction
{
    public string Key => "move_player_through_exit";

    public string DisplayName => "Move Player Through Exit";

    public string Description => "Moves the player through an exit to the connected scene";

    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "exit",
            DisplayName = "Exit",
            ParameterType = "gameElement",
            IsOptional = false,
            Description = "The exit element to traverse"
        }
    ];

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("exit", out var exitRef))
        {
            throw new InvalidOperationException("MovePlayerThroughExitEffect requires 'exit' parameter");
        }

        // Find the exit element using resolver
        var exitElement = ElementReferenceResolver.ResolveElement(exitRef, round, session);
        if (exitElement is not Exit exit)
        {
            throw new InvalidOperationException($"Element '{exitRef}' is not an Exit element");
        }

        // Find the target exit
        if (exit.TargetExitId == null)
        {
            throw new InvalidOperationException($"Exit '{exit.Name}' has no target exit configured");
        }

        var targetExit = session.Elements.OfType<Exit>().FirstOrDefault(e => e.Id == exit.TargetExitId);
        if (targetExit == null)
        {
            throw new InvalidOperationException($"Target exit for '{exit.Name}' not found");
        }

        // Find the target scene (parent of the target exit)
        if (targetExit.ParentId == null)
        {
            throw new InvalidOperationException($"Target exit '{targetExit.Name}' has no parent scene");
        }

        var targetScene = session.Elements.OfType<Scene>().FirstOrDefault(s => s.Id == targetExit.ParentId);
        if (targetScene == null)
        {
            throw new InvalidOperationException($"Target scene for exit '{targetExit.Name}' not found");
        }

        // Find the player
        var player = session.Elements.OfType<Player>().FirstOrDefault();
        if (player == null)
        {
            throw new InvalidOperationException("Player not found in session");
        }

        // Move the player to the target scene
        player.ParentId = targetScene.Id;

        return Task.CompletedTask;
    }
}
