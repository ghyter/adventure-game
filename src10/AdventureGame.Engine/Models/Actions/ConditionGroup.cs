#nullable enable
using System.Text.Json.Serialization;
using AdventureGame.Engine.Conditions;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Discriminated wrapper for either a parameter-based ConditionDefinition or a nested ConditionGroup.
/// </summary>
public sealed class ConditionNode
{
    /// <summary>
    /// Parameter-based condition definition
    /// </summary>
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ConditionDefinition? Definition { get; set; }
    
    /// <summary>
    /// Nested condition group for complex logic
    /// </summary>
    [JsonInclude]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ConditionGroup? ConditionGroup { get; set; }

    [JsonIgnore]
    public bool IsLeaf => Definition is not null;

    [JsonIgnore]
    public bool IsGroup => ConditionGroup is not null;

    public static ConditionNode FromDefinition(ConditionDefinition definition) => new() { Definition = definition };
    public static ConditionNode FromGroup(ConditionGroup group) => new() { ConditionGroup = group };
}

/// <summary>
/// A group of conditions combined with a LogicOperator (And/Or).
/// Supports nested condition groups for complex boolean logic.
/// </summary>
public sealed class ConditionGroup
{
    /// <summary>
    /// Unique identifier for this condition group
    /// </summary>
    [JsonInclude]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// How the conditions combine: And (all must be true) or Or (at least one must be true)
    /// </summary>
    [JsonInclude]
    public LogicOperator Operator { get; set; } = LogicOperator.And;
    
    /// <summary>
    /// The condition nodes in this group (can be individual conditions or nested groups)
    /// </summary>
    [JsonInclude]
    public List<ConditionNode> Nodes { get; set; } = [];
}
