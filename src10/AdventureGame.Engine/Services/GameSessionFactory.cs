namespace AdventureGame.Engine.Services;

using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;
using AdventureGame.Engine.Runtime;
using System.Text.Json;

/// <summary>
/// Default implementation of IGameSessionFactory.
/// </summary>
public class GameSessionFactory : IGameSessionFactory
{
    /// <summary>
    /// Creates a sandbox GameSession from a GamePack for testing.
    /// </summary>
    public GameSession CreateSandboxSession(GamePack pack)
    {
        if (pack == null)
            throw new ArgumentNullException(nameof(pack));

        // Deep copy the GamePack
        var packCopy = DeepCopyGamePack(pack);

        // Initialize all elements to their default state
        if (packCopy.Elements != null)
        {
            foreach (var element in packCopy.Elements)
            {
                InitializeElementToDefaultState(element);
            }
        }

        // Create a new GameSession using the public factory method
        var session = GameSession.NewGame(packCopy);

        return session;
    }

    /// <summary>
    /// Deep copies a GamePack using JSON serialization.
    /// </summary>
    private GamePack DeepCopyGamePack(GamePack pack)
    {
        var json = JsonSerializer.Serialize(pack);
        var copy = JsonSerializer.Deserialize<GamePack>(json);
        return copy ?? throw new InvalidOperationException("Failed to deep copy GamePack");
    }

    /// <summary>
    /// Initializes an element to its default state.
    /// </summary>
    private void InitializeElementToDefaultState(GameElement element)
    {
        if (element == null) return;

        // Set the current state to the default state
        var defaultState = element.DefaultState;
        if (!string.IsNullOrWhiteSpace(defaultState) && element.States.ContainsKey(defaultState))
        {
            // Store current state in Properties for runtime use
            element.Properties["CurrentState"] = defaultState;
        }

        // Initialize inventory for items if needed
        if (element is Item item)
        {
            // Ensure inventory list exists
            if (!element.Properties.ContainsKey("Inventory"))
            {
                element.Properties["Inventory"] = "[]";
            }
        }
    }
}
