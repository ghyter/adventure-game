namespace AdventureGame.Engine.DSL;

using AdventureGame.Engine.DSL.Parser;
using AdventureGame.Engine.DSL.Tokenizer;
using AdventureGame.Engine.DSL.Validation;
using AdventureGame.Engine.DSL.Evaluation;
using AdventureGame.Engine.DSL.AST;
using System.Text.Json;

/// <summary>
/// Main facade for DSL parsing, validation, and evaluation.
/// </summary>
public class DslService(DslSemanticValidator? validator = null)
{
    private readonly DslParser _parser = new();
    private readonly DslSemanticValidator? _validator = validator;

    /// <summary>
    /// Parses and optionally validates a DSL expression.
    /// </summary>
    public DslParseResult ParseAndValidate(string dslText)
    {
        // Parse
        var result = _parser.Parse(dslText);

        // Validate
        if (result.Success && _validator != null && result.Ast is not null)
        {
            _validator.Validate(result.Ast);
        }

        return result;
    }

    /// <summary>
    /// Converts AST to JSON for display.
    /// </summary>
    public string AstToJson(ConditionNode? ast)
    {
        if (ast == null) return "null";

        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(AstToObject(ast), options);
    }

    private object AstToObject(ConditionNode node)
    {
        return node switch
        {
            AndNode and => new
            {
                type = "AND",
                left = AstToObject(and.Left),
                right = AstToObject(and.Right)
            },
            OrNode or => new
            {
                type = "OR",
                left = AstToObject(or.Left),
                right = AstToObject(or.Right)
            },
            NotNode not => new
            {
                type = "NOT",
                inner = AstToObject(not.Inner)
            },
            RelationNode rel => new
            {
                type = "RELATION",
                subject = rel.Subject.ToString(),
                relation = rel.Relation,
                obj = rel.Object.ToString(),
                attribute = rel.AttributeName,
                property = rel.PropertyName
            },
            CountRelationNode count => new
            {
                type = "COUNT",
                subject = count.Subject.ToString(),
                scene = count.SceneName,
                comparison = count.Comparison,
                value = count.Value
            },
            DistanceRelationNode dist => new
            {
                type = "DISTANCE",
                subjectA = dist.SubjectA.ToString(),
                subjectB = dist.SubjectB.ToString(),
                comparison = dist.Comparison,
                value = dist.Value
            },
            _ => new { type = "UNKNOWN" }
        };
    }

    /// <summary>
    /// Evaluates an AST against game state.
    /// </summary>
    public bool Evaluate(ConditionNode ast, DslEvaluationContext context)
    {
        var evaluator = new DslEvaluator(context);
        return evaluator.Evaluate(ast);
    }
}
