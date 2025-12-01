namespace AdventureGame.Engine.Effects;

/// <summary>
/// Catalog of all available effect actions in the system.
/// Provides discovery and lookup of effect actions for both runtime execution and editor UI.
/// </summary>
public interface IEffectActionCatalog
{
    /// <summary>
    /// All registered effect actions in the system
    /// </summary>
    IReadOnlyList<IEffectAction> All { get; }
    
    /// <summary>
    /// Retrieves an effect action by its unique key
    /// </summary>
    /// <param name="key">The effect action key (e.g., "setProperty", "move")</param>
    /// <returns>The effect action, or null if not found</returns>
    IEffectAction? GetByKey(string key);
}
