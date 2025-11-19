namespace AdventureGame.Engine.DSL.AST;

/// <summary>
/// Reference to a game element subject (player, item, npc, scene, etc.).
/// </summary>
public class SubjectRef
{
    /// <summary>
    /// The kind of subject: "player", "item", "npc", "scene", "exit", "target", "currentScene", "session", "log", "target2"
    /// </summary>
    public string Kind { get; set; } = null!;

    /// <summary>
    /// The resolved ID or name of the element. Null for singleton subjects like "player" or "session".
    /// </summary>
    public string? Id { get; set; }

    public override string ToString() => Id != null ? $"{Kind}({Id})" : Kind;
}

/// <summary>
/// Reference to an object value (game element, literal, etc.).
/// </summary>
public class ObjectRef
{
    /// <summary>
    /// The kind: "element", "literal", "inventory", "property", "attribute", "flag", "state"
    /// </summary>
    public string Kind { get; set; } = null!;

    /// <summary>
    /// The value: either a resolved ID or a literal string.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Optional numeric value if this is a number.
    /// </summary>
    public double? NumericValue { get; set; }

    /// <summary>
    /// True if this is a boolean literal (true/false).
    /// </summary>
    public bool? BoolValue { get; set; }

    public override string ToString() => $"{Kind}({Value})";
}

/// <summary>
/// Represents a relation between a subject, a comparison operator, and an object.
/// Examples: "player.inventory is_empty", "target.state is open", "player.attribute constitution is_less_than 3"
/// </summary>
public class RelationNode : ConditionNode
{
    /// <summary>
    /// The subject being tested (e.g., "player", "item jade_key", "npc monster").
    /// </summary>
    public SubjectRef Subject { get; set; } = null!;

    /// <summary>
    /// The name of the relation: "is_empty", "is_in", "is", "is_not", "is_less_than", "is_greater_than", "is_equal_to", "is_not_equal_to"
    /// </summary>
    public string Relation { get; set; } = null!;

    /// <summary>
    /// The object being compared against.
    /// </summary>
    public ObjectRef Object { get; set; } = null!;

    /// <summary>
    /// Optional: the name of the attribute being accessed (e.g., "constitution" in "player.attribute constitution is_less_than 3").
    /// </summary>
    public string? AttributeName { get; set; }

    /// <summary>
    /// Optional: the name of the property being accessed.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>
    /// Optional: numeric value extracted from the Object.
    /// </summary>
    public double? NumericValue { get; set; }

    public override string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString() => $"{Subject} {Relation} {Object}";
}

/// <summary>
/// Represents a "visits" relation: counting how many times a subject has visited a scene.
/// Example: "player.visits kitchen is_greater_than 3"
/// </summary>
public class CountRelationNode : ConditionNode
{
    /// <summary>
    /// The subject (e.g., "player").
    /// </summary>
    public SubjectRef Subject { get; set; } = null!;

    /// <summary>
    /// The name of the scene being visited.
    /// </summary>
    public string SceneName { get; set; } = null!;

    /// <summary>
    /// The comparison operator: "is_less_than", "is_greater_than", "is_equal_to", "is_not_equal_to"
    /// </summary>
    public string Comparison { get; set; } = null!;

    /// <summary>
    /// The numeric value to compare against.
    /// </summary>
    public int Value { get; set; }

    public override string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString() => $"{Subject}.visits {SceneName} {Comparison} {Value}";
}

/// <summary>
/// Represents a distance relation: comparing the distance between two subjects.
/// Example: "npc monster.distance_from player is_greater_than 2"
/// </summary>
public class DistanceRelationNode : ConditionNode
{
    /// <summary>
    /// The first subject.
    /// </summary>
    public SubjectRef SubjectA { get; set; } = null!;

    /// <summary>
    /// The second subject to measure distance from.
    /// </summary>
    public SubjectRef SubjectB { get; set; } = null!;

    /// <summary>
    /// The comparison operator.
    /// </summary>
    public string Comparison { get; set; } = null!;

    /// <summary>
    /// The numeric distance value to compare against.
    /// </summary>
    public int Value { get; set; }

    public override string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }

    public override string ToString() => $"{SubjectA}.distance_from {SubjectB} {Comparison} {Value}";
}
