namespace AdventureGame.Engine.Services;

using AdventureGame.Engine.Models;
using AdventureGame.Engine.Runtime;

/// <summary>
/// Factory for creating GameSession instances.
/// </summary>
public interface IGameSessionFactory
{
    /// <summary>
    /// Creates a sandbox GameSession from a GamePack for testing purposes.
    /// Deep-copies all elements and initializes them to their default state.
    /// </summary>
    GameSession CreateSandboxSession(GamePack pack);
}
