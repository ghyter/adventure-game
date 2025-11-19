namespace AdventureGame.Engine.Tests.DSL;

using AdventureGame.Engine.DSL;
using AdventureGame.Engine.DSL.Parser;
using AdventureGame.Engine.DSL.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Basic tests for DSL parsing, validation, and evaluation.
/// </summary>
[TestClass]
public class DslParserTests
{
    [TestMethod]
    public void Parse_SimpleComparison_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("player is target");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_AndCondition_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("player is target and target.state is open");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_OrCondition_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("item jade_key is_in player or item silver_key is_in player");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_NotCondition_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("not target.state is open");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_ParenthesizedExpression_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("(player is target or player is target2) and not target.state is locked");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_VisitsClause_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("player.visits kitchen is_greater_than 3");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_DistanceClause_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("npc monster.distance_from player is_greater_than 2");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_AttributeComparison_Succeeds()
    {
        var parser = new DslParser();
        var result = parser.Parse("player.attribute constitution is_less_than 3");

        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_InvalidSyntax_Fails()
    {
        var parser = new DslParser();
        var result = parser.Parse("player is");

        Assert.IsFalse(result.Success);
        Assert.IsNotEmpty(result.Errors);
    }

    [TestMethod]
    public void AstToJson_ProducesValidJson()
    {
        var parser = new DslParser();
        var result = parser.Parse("player is target and target.state is open");
        Assert.IsTrue(result.Success);

        var service = new DslService();
        var json = service.AstToJson(result.Ast);

        Assert.IsNotNull(json);
        Assert.Contains("AND", json, "JSON should contain node type");
    }

    [TestMethod]
    public void MockEvaluation_SimpleCondition_Evaluates()
    {
        var parser = new DslParser();
        var result = parser.Parse("true");
        Assert.IsTrue(result.Success);

        var mockContext = new MockDslEvaluationContext();
        var service = new DslService();
        bool evalResult = service.Evaluate(result.Ast!, mockContext);

        Assert.IsTrue(evalResult);
    }
}

/// <summary>
/// Mock evaluation context for testing.
/// </summary>
public class MockDslEvaluationContext : DslEvaluationContext
{
    private readonly Dictionary<string, object?> _state = [];

    public MockDslEvaluationContext()
    {
        _state["player"] = "player_entity";
        _state["target"] = "target_entity";
    }

    public override object? GetPlayer() => _state.TryGetValue("player", out var p) ? p : null;
    public override object? GetTarget() => _state.TryGetValue("target", out var t) ? t : null;
    public override object? GetTarget2() => _state.TryGetValue("target2", out var t2) ? t2 : null;
    public override object? GetCurrentScene() => _state.TryGetValue("scene", out var s) ? s : null;
    public override object? GetSession() => _state.TryGetValue("session", out var sess) ? sess : null;
    public override object? GetLog() => _state.TryGetValue("log", out var l) ? l : null;
    public override object? GetElement(string kind, string? id) => null;
    public override int GetVisitCount(string subjectKind, string sceneName) => 0;
    public override int GetDistance(AdventureGame.Engine.DSL.AST.SubjectRef from, AdventureGame.Engine.DSL.AST.SubjectRef to) => 0;
}
