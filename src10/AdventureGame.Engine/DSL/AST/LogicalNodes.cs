namespace AdventureGame.Engine.DSL.AST;

/// <summary>
/// Represents a logical AND operation between two condition nodes.
/// </summary>
public class AndNode : ConditionNode
{
    public ConditionNode Left { get; set; } = null!;
    public ConditionNode Right { get; set; } = null!;

    public override string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
/// Represents a logical OR operation between two condition nodes.
/// </summary>
public class OrNode : ConditionNode
{
    public ConditionNode Left { get; set; } = null!;
    public ConditionNode Right { get; set; } = null!;

    public override string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
/// Represents a logical NOT operation on a condition node.
/// </summary>
public class NotNode : ConditionNode
{
    public ConditionNode Inner { get; set; } = null!;

    public override string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }
}
