using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Parameters;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Effects;

/// <summary>
/// Represents a discoverable, metadata-driven effect action.
/// Effect actions are executed during game rounds and can modify game state.
/// All effect actions must be registered in DI for catalog discovery.
/// </summary>
public interface IEffectAction
{
    /// <summary>
    /// Unique identifier for this effect action (e.g., "setProperty", "move", "rollDice")
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Human-readable name shown in the editor UI
    /// </summary>
    string DisplayName { get; }
    
    /// <summary>
    /// Detailed description of what this effect does
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// List of parameters this effect requires/accepts
    /// </summary>
    IReadOnlyList<ParameterDescriptor> Parameters { get; }
    
    /// <summary>
    /// Executes the effect action with the given parameters.
    /// Effects should modify the round state, not the session directly.
    /// Changes are committed after the round completes.
    /// </summary>
    /// <param name="round">The current game round containing targets and state</param>
    /// <param name="session">The game session containing world state</param>
    /// <param name="parameters">Effect-specific parameters from the definition</param>
    Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters);
}
