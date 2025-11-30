namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Catalog of all available condition operators in the system.
/// Provides discovery and lookup of condition operators for both runtime execution and editor UI.
/// </summary>
public interface IConditionOperatorCatalog
{
    /// <summary>
    /// All registered condition operators in the system
    /// </summary>
    IReadOnlyList<IConditionOperator> All { get; }
    
    /// <summary>
    /// Retrieves a condition operator by its unique key
    /// </summary>
    /// <param name="key">The condition operator key (e.g., "equals", "contains")</param>
    /// <returns>The condition operator, or null if not found</returns>
    IConditionOperator? GetByKey(string key);
}
