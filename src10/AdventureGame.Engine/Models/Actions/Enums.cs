#nullable enable

namespace AdventureGame.Engine.Models.Actions;

/// <summary>Logical operator for combining conditions in a group.</summary>
public enum LogicOperator
{
    And,
    Or
}

/// <summary>Comparison operator used by structured property path conditions.</summary>
public enum ComparisonOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    NotContains,
    Exists,
    NotExists
}

/// <summary>Effect operation applied to a target element's property/attribute/flag.</summary>
public enum EffectOperation
{
    Set,
    Increment,
    Decrement,
    Toggle,
    MoveTo,
    AddFlag,
    RemoveFlag
}
