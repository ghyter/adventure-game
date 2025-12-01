namespace AdventureGame.Engine.Parameters;

/// <summary>
/// Catalog of all available parameter type handlers in the system.
/// Provides discovery and lookup of parameter types for both runtime execution and editor UI.
/// </summary>
public interface IParameterCatalog
{
    /// <summary>
    /// All registered parameter type handlers in the system
    /// </summary>
    IEnumerable<IParameterTypeHandler> All { get; }
    
    /// <summary>
    /// Retrieves a parameter type handler by its unique key
    /// </summary>
    /// <param name="key">The parameter type key (e.g., "gameElement", "number")</param>
    /// <returns>The parameter type handler, or null if not found</returns>
    IParameterTypeHandler? Get(string key);
}
