#nullable enable
using System.Text.Json.Serialization;
using AdventureGame.Engine.Models.Round;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Discriminated wrapper for either a leaf Condition (legacy) or a nested ConditionGroup.
/// </summary>
public sealed class ConditionNode
{
    public Condition? Condition { get; set; }
    public ConditionGroup? ConditionGroup { get; set; }

    [JsonIgnore]
    public bool IsLeaf => Condition is not null;

    [JsonIgnore]
    public bool IsGroup => ConditionGroup is not null;

    public static ConditionNode FromCondition(Condition condition) => new() { Condition = condition };
    public static ConditionNode FromGroup(ConditionGroup group) => new() { ConditionGroup = group };
}

/// <summary>
/// A group of conditions combined with a LogicOperator (And/Or).
/// </summary>
public sealed class ConditionGroup
{
    public LogicOperator Operator { get; set; } = LogicOperator.And;
    public List<ConditionNode> Nodes { get; set; } = new();
}
