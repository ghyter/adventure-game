namespace AdventureGame.Engine.DSL.Validation;

using AdventureGame.Engine.DSL.AST;
using AdventureGame.Engine.DSL.Parser;

/// <summary>
/// Validates semantic correctness of a DSL AST.
/// </summary>
public class DslSemanticValidator(DslParseResult parseResult)
{
    private readonly HashSet<string> _validGameElements = [];
    private readonly HashSet<string> _validScenes = [];
    private readonly HashSet<string> _validAttributes = [];
    private readonly DslParseResult _result = parseResult;

    public void SetValidElements(IEnumerable<string> elements)
    {
        _validGameElements.Clear();
        _validGameElements.UnionWith(elements);
    }

    public void SetValidScenes(IEnumerable<string> scenes)
    {
        _validScenes.Clear();
        _validScenes.UnionWith(scenes);
    }

    public void SetValidAttributes(IEnumerable<string> attributes)
    {
        _validAttributes.Clear();
        _validAttributes.UnionWith(attributes);
    }

    public bool Validate(ConditionNode ast)
    {
        if (ast == null) return false;
        ValidateNode(ast);
        return _result.Success;
    }

    private void ValidateNode(ConditionNode node)
    {
        switch (node)
        {
            case AndNode and:
                ValidateNode(and.Left);
                ValidateNode(and.Right);
                break;
            case OrNode or:
                ValidateNode(or.Left);
                ValidateNode(or.Right);
                break;
            case NotNode not:
                ValidateNode(not.Inner);
                break;
            case RelationNode rel:
                ValidateRelation(rel);
                break;
            case CountRelationNode count:
                ValidateCountRelation(count);
                break;
            case DistanceRelationNode dist:
                ValidateDistanceRelation(dist);
                break;
        }
    }

    private void ValidateRelation(RelationNode node)
    {
        ValidateSubject(node.Subject);
        ValidateObject(node.Object);

        // Validate attribute if specified
        if (!string.IsNullOrEmpty(node.AttributeName))
        {
            if (!_validAttributes.Contains(node.AttributeName))
            {
                _result.AddError(
                    $"Unknown attribute: {node.AttributeName}",
                    0, 0, ErrorSeverity.Warning);
            }
        }
    }

    private void ValidateCountRelation(CountRelationNode node)
    {
        ValidateSubject(node.Subject);
        if (!_validScenes.Contains(node.SceneName))
        {
            _result.AddError(
                $"Unknown scene: {node.SceneName}",
                0, 0, ErrorSeverity.Warning);
        }
    }

    private void ValidateDistanceRelation(DistanceRelationNode node)
    {
        ValidateSubject(node.SubjectA);
        ValidateSubject(node.SubjectB);
    }

    private void ValidateSubject(SubjectRef subject)
    {
        if (subject == null) return;

        if (!string.IsNullOrEmpty(subject.Id))
        {
            if (!_validGameElements.Contains(subject.Id))
            {
                _result.AddError(
                    $"Unknown element: {subject.Id}",
                    0, 0, ErrorSeverity.Warning);
            }
        }
    }

    private void ValidateObject(ObjectRef obj)
    {
        if (obj == null) return;

        if (obj.Kind == "element" && !_validGameElements.Contains(obj.Value))
        {
            _result.AddError(
                $"Unknown element: {obj.Value}",
                0, 0, ErrorSeverity.Warning);
        }
    }
}
