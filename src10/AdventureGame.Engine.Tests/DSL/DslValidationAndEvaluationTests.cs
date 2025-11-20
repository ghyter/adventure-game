namespace AdventureGame.Engine.Tests.DSL;

using AdventureGame.Engine.DSL;
using AdventureGame.Engine.DSL.Parser;
using AdventureGame.Engine.DSL.Validation;
using AdventureGame.Engine.DSL.Evaluation;
using AdventureGame.Engine.DSL.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Comprehensive test suite for DSL Semantic Validator.
/// </summary>
[TestClass]
public class DslSemanticValidatorTests
{
    private DslParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new DslParser();
    }

    [TestMethod]
    public void Validate_KnownElement_Passes()
    {
        var result = _parser.Parse("player is target");
        var validator = new DslSemanticValidator(result);
        validator.SetValidElements(["player", "target"]);
        bool isValid = validator.Validate(result.Ast!);
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public void Validate_UnknownElement_GeneratesWarning()
    {
        var result = _parser.Parse("player is unknown_element");
        var validator = new DslSemanticValidator(result);
        validator.SetValidElements(["player"]);
        validator.Validate(result.Ast!);
        Assert.IsTrue(result.Errors.Any(e => e.Severity == ErrorSeverity.Warning));
    }

    [TestMethod]
    public void Validate_UnknownAttribute_GeneratesWarning()
    {
        var result = _parser.Parse("player.attribute unknown_attr is_less_than 5");
        var validator = new DslSemanticValidator(result);
        validator.SetValidAttributes(["constitution", "strength"]);
        validator.Validate(result.Ast!);
        Assert.IsTrue(result.Errors.Any(e => e.Severity == ErrorSeverity.Warning));
    }

    [TestMethod]
    public void Validate_UnknownScene_GeneratesWarning()
    {
        var result = _parser.Parse("player.visits unknown_scene is_greater_than 0");
        var validator = new DslSemanticValidator(result);
        validator.SetValidScenes(["kitchen", "bedroom"]);
        validator.Validate(result.Ast!);
        Assert.IsTrue(result.Errors.Any(e => e.Severity == ErrorSeverity.Warning));
    }

    [TestMethod]
    public void Validate_ComplexExpression()
    {
        var dsl = "(player has jade_key or player has silver_key) and player.visits treasury is_greater_than 0";
        var result = _parser.Parse(dsl);
        var validator = new DslSemanticValidator(result);
        validator.SetValidElements(["jade_key", "silver_key"]);
        validator.SetValidScenes(["treasury"]);
        validator.Validate(result.Ast!);
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Validate_Traverses_AndNode()
    {
        var and = new AndNode
        {
            Left = new RelationNode { Subject = new SubjectRef { Kind = "player", Id = "unknown1" } },
            Right = new RelationNode { Subject = new SubjectRef { Kind = "player", Id = "unknown2" } }
        };
        var result = new DslParseResult { Success = true, Ast = and };
        var validator = new DslSemanticValidator(result);
        validator.SetValidElements(["player"]);
        validator.Validate(and);
#pragma warning disable MSTEST0037
        Assert.IsTrue(result.Errors.Count > 0);
#pragma warning restore MSTEST0037
    }

    [TestMethod]
    public void Validate_CountRelation_UnknownScene()
    {
        var count = new CountRelationNode
        {
            Subject = new SubjectRef { Kind = "player" },
            SceneName = "unknown_scene",
            Comparison = "is_greater_than",
            Value = 1
        };
        var result = new DslParseResult { Success = true, Ast = count };
        var validator = new DslSemanticValidator(result);
        validator.SetValidScenes(["kitchen"]);
        validator.Validate(count);
        Assert.IsTrue(result.Errors.Any(e => e.Message.Contains("unknown_scene")));
    }
}

/// <summary>
/// Comprehensive test suite for DSL Evaluator.
/// </summary>
[TestClass]
public class DslEvaluatorTests
{
    [TestMethod]
    public void Evaluate_RelationNode_SimpleComparison()
    {
        var rel = new RelationNode
        {
            Subject = new SubjectRef { Kind = "player" },
            Relation = "is",
            Object = new ObjectRef { Kind = "element", Value = "target" }
        };
        // Make player resolve to the same value as target for equality
        var context = new TestEvaluationContext { Player = "target", Target = "target" };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(rel);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_AndNode_BothTrue()
    {
        var and = new AndNode
        {
            Left = new RelationNode
            {
                Subject = new SubjectRef { Kind = "player" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "player" }
            },
            Right = new RelationNode
            {
                Subject = new SubjectRef { Kind = "target" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "target" }
            }
        };
        var context = new TestEvaluationContext { Player = "player", Target = "target" };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(and);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_AndNode_LeftTrue_RightFalse()
    {
        var and = new AndNode
        {
            Left = new RelationNode
            {
                Subject = new SubjectRef { Kind = "player" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "player" }
            },
            Right = new RelationNode
            {
                Subject = new SubjectRef { Kind = "player" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "target" }
            }
        };
        var context = new TestEvaluationContext { Player = "player", Target = "target" };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(and);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Evaluate_OrNode_OneTrue()
    {
        var or = new OrNode
        {
            Left = new RelationNode
            {
                Subject = new SubjectRef { Kind = "player" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "player" }
            },
            Right = new RelationNode
            {
                Subject = new SubjectRef { Kind = "player" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "other" }
            }
        };
        var context = new TestEvaluationContext { Player = "player" };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(or);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_NotNode_InvertsTrue()
    {
        var not = new NotNode
        {
            Inner = new RelationNode
            {
                Subject = new SubjectRef { Kind = "player" },
                Relation = "is",
                Object = new ObjectRef { Kind = "element", Value = "player" }
            }
        };
        var context = new TestEvaluationContext { Player = "player" };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(not);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Evaluate_NumericComparison_LessThan()
    {
        var rel = new RelationNode
        {
            Subject = new SubjectRef { Kind = "player" },
            Relation = "is_less_than",
            Object = new ObjectRef { Kind = "literal", Value = "10", NumericValue = 10.0 }
        };
        // Provide a numeric player value directly for comparison
        var context = new TestEvaluationContext { Player = 5.0 };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(rel);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_CountRelation()
    {
        var count = new CountRelationNode
        {
            Subject = new SubjectRef { Kind = "player" },
            SceneName = "kitchen",
            Comparison = "is_greater_than",
            Value = 3
        };
        var context = new TestEvaluationContext { VisitCount = 5 };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(count);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_ComplexExpression()
    {
        var and = new AndNode
        {
            Left = new OrNode
            {
                Left = new RelationNode
                {
                    Subject = new SubjectRef { Kind = "player" },
                    Relation = "is",
                    Object = new ObjectRef { Kind = "element", Value = "player" }
                },
                Right = new RelationNode
                {
                    Subject = new SubjectRef { Kind = "player" },
                    Relation = "is",
                    Object = new ObjectRef { Kind = "element", Value = "target" }
                }
            },
            Right = new NotNode
            {
                Inner = new RelationNode
                {
                    Subject = new SubjectRef { Kind = "player" },
                    Relation = "is",
                    Object = new ObjectRef { Kind = "element", Value = "other" }
                }
            }
        };
        var context = new TestEvaluationContext { Player = "player" };
        var evaluator = new DslEvaluator(context);
        bool result = evaluator.Evaluate(and);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_NullContext_Throws()
    {
        var rel = new RelationNode();
        try
        {
            var evaluator = new DslEvaluator(null!);
            evaluator.Evaluate(rel);
            Assert.Fail("Should have thrown ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    private class TestEvaluationContext : DslEvaluationContext
    {
        public object? Player { get; set; }
        public object? Target { get; set; }
        public double PlayerValue { get; set; }
        public int VisitCount { get; set; }

        public override object? GetPlayer() => Player;
        public override object? GetTarget() => Target;
        public override object? GetTarget2() => null;
        public override object? GetCurrentScene() => null;
        public override object? GetSession() => null;
        public override object? GetLog() => null;
        public override object? GetElement(string kind, string? id) => null;
        public override int GetVisitCount(string subjectKind, string sceneName) => VisitCount;
        public override int GetDistance(SubjectRef from, SubjectRef to) => 0;
    }
}

/// <summary>
/// Integration tests for the complete DSL pipeline.
/// </summary>
[TestClass]
public class DslIntegrationTests
{
    private DslService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        var vocab = new DslVocabulary();
        _service = new DslService(vocab);
    }

    [TestMethod]
    public void FullPipeline_ParseValidateEvaluate()
    {
        var dsl = "player is target and target.state is open";
        var result = _service.ParseAndValidate(dsl);
        Assert.IsTrue(result.Success);
        var context = new TestEvaluationContext { Player = "target", Target = "target" };
        bool evalResult = DslService.Evaluate(result.Ast!, context);
        Assert.IsTrue(evalResult);
    }

    [TestMethod]
    public void AstToJson_ProducesValidStructure()
    {
        var dsl = "player is target and not target.state is locked";
        var result = _service.ParseAndValidate(dsl);
        string json = DslService.AstToJson(result.Ast);
        Assert.IsNotNull(json);
        Assert.Contains("AND", json);
        Assert.Contains("NOT", json);
    }

    [TestMethod]
    public void ErrorHandling_SyntaxError_ReportsPosition()
    {
        var dsl = "player is";
        var result = _service.ParseAndValidate(dsl);
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Errors.Any(e => e.StartIndex >= 0));
    }

    private class TestEvaluationContext : DslEvaluationContext
    {
        public object? Player { get; set; }
        public object? Target { get; set; }

        public override object? GetPlayer() => Player;
        public override object? GetTarget() => Target;
        public override object? GetTarget2() => null;
        public override object? GetCurrentScene() => null;
        public override object? GetSession() => null;
        public override object? GetLog() => null;
        public override object? GetElement(string kind, string? id) => null;
        public override int GetVisitCount(string subjectKind, string sceneName) => 0;
        public override int GetDistance(SubjectRef from, SubjectRef to) => 0;
    }
}

/// <summary>
/// Edge case and boundary tests for the DSL system.
/// </summary>
[TestClass]
public class DslEdgeCaseTests
{
    private DslService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        var vocab = new DslVocabulary();
        _service = new DslService(vocab);
    }

    [TestMethod]
    public void Parse_VeryLongIdentifier()
    {
        var longId = new string('a', 1000);
        var result = _service.ParseAndValidate($"player is {longId}");
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_LargeNumber()
    {
        var result = _service.ParseAndValidate("player.health is_greater_than 999999999");
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_DeeplyNestedParentheses()
    {
        var dsl = "((((player is target))))";
        var result = _service.ParseAndValidate(dsl);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_ManyAndOperators()
    {
        var dsl = "player is a and player is b and player is c and player is d and player is e";
        var result = _service.ParseAndValidate(dsl);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_ManyOrOperators()
    {
        var dsl = "player is a or player is b or player is c or player is d or player is e";
        var result = _service.ParseAndValidate(dsl);
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_NegativeNumbers()
    {
        var result = _service.ParseAndValidate("player.health is_greater_than -10");
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_DecimalNumbers()
    {
        var result = _service.ParseAndValidate("player.mana is_less_than 3.5");
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void ErrorMessages_AreClear()
    {
        var result = _service.ParseAndValidate("player is");
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Errors.All(e => !string.IsNullOrEmpty(e.Message)));
    }
}
