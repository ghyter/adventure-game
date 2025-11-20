namespace AdventureGame.Engine.DSL;

using AdventureGame.Engine.DSL.Parser;
using AdventureGame.Engine.DSL.Tokenizer;
using AdventureGame.Engine.DSL.Validation;
using AdventureGame.Engine.DSL.Evaluation;
using AdventureGame.Engine.DSL.AST;
using System.Text.Json;

/// <summary>
/// Main facade for DSL parsing, validation, and evaluation.
/// Integrates canonicalization, caching, and semantic validation.
/// </summary>
/// <remarks>
/// Creates a new DslService with vocabulary and optional validator.
/// </remarks>
public class DslService(DslVocabulary vocab, IDslCanonicalizer? canonicalizer = null, DslSemanticValidator? validator = null)
{
    private readonly DslParser _parser = new();
    private readonly DslSemanticValidator? _validator = validator;
    private readonly IDslCanonicalizer _canonicalizer = canonicalizer ?? new DslCanonicalizer();
    private readonly DslVocabulary _vocab = vocab ?? throw new ArgumentNullException(nameof(vocab));
    private readonly CompiledExpressionCache _cache = new();

    /// <summary>
    /// Parses and optionally validates a DSL expression.
    /// Automatically canonicalizes the input and uses caching.
    /// </summary>
    public DslParseResult ParseAndValidate(string dslText)
    {
        if (string.IsNullOrWhiteSpace(dslText))
            return new DslParseResult { Success = false };

        // Canonicalize the input
        var canonical = _canonicalizer.Canonicalize(dslText, _vocab);

        // Parse (using cache)
        var result = _parser.Parse(canonical);

        // Validate if we have a validator
        if (result.Success && _validator != null && result.Ast is not null)
        {
            _validator.Validate(result.Ast);
        }

        return result;
    }

    /// <summary>
    /// Evaluates DSL text against game state.
    /// Canonicalizes, parses, and evaluates with caching.
    /// </summary>
    public bool EvaluateText(string dslText, DslEvaluationContext context)
    {
        if (string.IsNullOrWhiteSpace(dslText) || context == null)
            return false;

        // Canonicalize the input
        var canonical = _canonicalizer.Canonicalize(dslText, _vocab);

        // Get or compile the AST (with caching)
        var ast = _cache.GetOrAddCondition(canonical, c => _parser.Parse(c).Ast);
        
        if (ast == null) return false;

        // Evaluate
        return Evaluate(ast, context);
    }

    /// <summary>
    /// Converts AST to JSON for display.
    /// </summary>
    public static string AstToJson(ConditionNode? ast)
    {
        if (ast == null) return "null";

        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(AstToObject(ast), options);
    }

    private static object AstToObject(ConditionNode node)
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
    public static bool Evaluate(ConditionNode ast, DslEvaluationContext context)
    {
        var evaluator = new DslEvaluator(context);
        return evaluator.Evaluate(ast);
    }
}
