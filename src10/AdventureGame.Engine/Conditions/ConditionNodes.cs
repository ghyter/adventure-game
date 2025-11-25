namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Base class for all condition AST nodes.
/// </summary>
public abstract class ConditionNode
{
    /// <summary>
    /// The subject being tested. Could be "player", "target", "currentScene", etc.
    /// </summary>
    public string Subject { get; init; } = "";
}

/// <summary>
/// Checks if an element is of a specific kind (type).
/// Example: "when target is exit"
/// </summary>
public sealed class KindCheckCondition : ConditionNode
{
    public string Kind { get; init; } = "";
}

/// <summary>
/// Checks if an element is in a specific state.
/// Example: "when target is open"
/// </summary>
public sealed class StateCheckCondition : ConditionNode
{
    public string StateName { get; init; } = "";
}

/// <summary>
/// Checks if a boolean flag has a specific value.
/// Example: "when target visible is true"
/// </summary>
public sealed class FlagCheckCondition : ConditionNode
{
    public string FlagName { get; init; } = "";
    public bool ExpectedValue { get; init; }
}

/// <summary>
/// Checks if a property has a specific value.
/// Example: "when target description is 'old door'"
/// </summary>
public sealed class PropertyCheckCondition : ConditionNode
{
    public string PropertyName { get; init; } = "";
    public string ExpectedValue { get; init; } = "";
}

/// <summary>
/// Checks if an attribute has a specific value.
/// Example: "when player health is 10"
/// </summary>
public sealed class AttributeCheckCondition : ConditionNode
{
    public string AttributeName { get; init; } = "";
    public int ExpectedValue { get; init; }
}

/// <summary>
/// Performs numeric comparison on an attribute or property.
/// Example: "when player health > 5"
/// </summary>
public sealed class ComparisonCondition : ConditionNode
{
    public string Key { get; init; } = "";
    public ComparisonOperator Operator { get; init; }
    public int Value { get; init; }
}

public enum ComparisonOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}

/// <summary>
/// Logical AND of two conditions.
/// Example: "when player health > 5 and player has key"
/// </summary>
public sealed class LogicalAnd : ConditionNode
{
    public ConditionNode Left { get; init; } = null!;
    public ConditionNode Right { get; init; } = null!;
}

/// <summary>
/// Logical OR of two conditions.
/// Example: "when player has key or player has lockpick"
/// </summary>
public sealed class LogicalOr : ConditionNode
{
    public ConditionNode Left { get; init; } = null!;
    public ConditionNode Right { get; init; } = null!;
}

/// <summary>
/// Logical NOT of a condition.
/// Example: "when not player has key"
/// </summary>
public sealed class LogicalNot : ConditionNode
{
    public ConditionNode Inner { get; init; } = null!;
}

/// <summary>
/// Checks if a subject has a specific item.
/// Example: "when player has key"
/// </summary>
public sealed class HasItemCondition : ConditionNode
{
    public string ItemName { get; init; } = "";
}
