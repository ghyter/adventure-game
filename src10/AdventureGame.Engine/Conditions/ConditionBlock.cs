using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Represents a logical block of conditions with AND/OR/NOT semantics.
/// Supports nested condition evaluation.
/// </summary>
public sealed class ConditionBlock
{
    /// <summary>
    /// All of these conditions must be true (AND logic)
    /// </summary>
    [JsonInclude]
    public List<ConditionDefinition> AllOf { get; set; } = [];
    
    /// <summary>
    /// At least one of these conditions must be true (OR logic)
    /// </summary>
    [JsonInclude]
    public List<ConditionDefinition> AnyOf { get; set; } = [];
    
    /// <summary>
    /// None of these conditions must be true (NOT logic)
    /// </summary>
    [JsonInclude]
    public List<ConditionDefinition> NoneOf { get; set; } = [];
    
    /// <summary>
    /// Nested condition blocks for complex logic
    /// </summary>
    [JsonInclude]
    public List<ConditionBlock> NestedBlocks { get; set; } = [];
}
