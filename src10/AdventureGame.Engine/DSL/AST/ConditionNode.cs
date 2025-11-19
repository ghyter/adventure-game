namespace AdventureGame.Engine.DSL.AST;

/// <summary>
/// Base abstract class for all condition AST nodes.
/// </summary>
public abstract class ConditionNode
{
    public virtual string Accept(INodeVisitor visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
/// Interface for visitor pattern on AST nodes.
/// </summary>
public interface INodeVisitor
{
    string Visit(ConditionNode node);
    string Visit(AndNode node);
    string Visit(OrNode node);
    string Visit(NotNode node);
    string Visit(RelationNode node);
    string Visit(CountRelationNode node);
    string Visit(DistanceRelationNode node);
}
