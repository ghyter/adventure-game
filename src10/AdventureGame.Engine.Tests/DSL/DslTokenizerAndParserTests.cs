namespace AdventureGame.Engine.Tests.DSL;

using AdventureGame.Engine.DSL;
using AdventureGame.Engine.DSL.Parser;
using AdventureGame.Engine.DSL.Tokenizer;
using AdventureGame.Engine.DSL.Validation;
using AdventureGame.Engine.DSL.Evaluation;
using AdventureGame.Engine.DSL.AST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

/// <summary>
/// Comprehensive test suite for DSL Tokenizer.
/// </summary>
[TestClass]
public class DslTokenizerTests
{
    [TestMethod]
    public void Tokenize_SingleKeyword_ReturnsCorrectTokenType()
    {
        var tokenizer = new DslTokenizer("and");
        var tokens = tokenizer.Tokenize();

        Assert.HasCount(2, tokens); // "and" + EndOfInput
        Assert.AreEqual(TokenType.And, tokens[0].Type);
        Assert.AreEqual("and", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_AllKeywords_RecognizedCorrectly()
    {
        var keywords = new[]
        {
            ("and", TokenType.And),
            ("or", TokenType.Or),
            ("not", TokenType.Not),
            ("is", TokenType.Is),
            ("is_not", TokenType.IsNot),
            ("is_less_than", TokenType.IsLessThan),
            ("is_greater_than", TokenType.IsGreaterThan),
            ("is_equal_to", TokenType.IsEqualTo),
            ("is_not_equal_to", TokenType.IsNotEqualTo),
            ("is_in", TokenType.IsIn),
            ("is_empty", TokenType.IsEmpty),
            ("true", TokenType.True),
            ("false", TokenType.False),
        };

        foreach (var (keyword, expectedType) in keywords)
        {
            var tokenizer = new DslTokenizer(keyword);
            var tokens = tokenizer.Tokenize();
            Assert.AreEqual(expectedType, tokens[0].Type, $"Failed for keyword: {keyword}");
        }
    }

    [TestMethod]
    public void Tokenize_Identifier_RecognizedCorrectly()
    {
        var tokenizer = new DslTokenizer("player_name");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(TokenType.Identifier, tokens[0].Type);
        Assert.AreEqual("player_name", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_Number_Integer_RecognizedCorrectly()
    {
        var tokenizer = new DslTokenizer("42");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(TokenType.Number, tokens[0].Type);
        Assert.AreEqual("42", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_Number_Decimal_RecognizedCorrectly()
    {
        var tokenizer = new DslTokenizer("3.14");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(TokenType.Number, tokens[0].Type);
        Assert.AreEqual("3.14", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_Number_Negative_RecognizedCorrectly()
    {
        var tokenizer = new DslTokenizer("-42");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(TokenType.Number, tokens[0].Type);
        Assert.AreEqual("-42", tokens[0].Value);
    }

    [TestMethod]
    public void Tokenize_Parentheses_RecognizedCorrectly()
    {
        var tokenizer = new DslTokenizer("(test)");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(TokenType.ParenOpen, tokens[0].Type);
        Assert.AreEqual(TokenType.Identifier, tokens[1].Type);
        Assert.AreEqual(TokenType.ParenClose, tokens[2].Type);
    }

    [TestMethod]
    public void Tokenize_Dot_RecognizedCorrectly()
    {
        var tokenizer = new DslTokenizer("player.state");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(TokenType.Identifier, tokens[0].Type);
        Assert.AreEqual(TokenType.Dot, tokens[1].Type);
        Assert.AreEqual(TokenType.Identifier, tokens[2].Type);
    }

    [TestMethod]
    public void Tokenize_IgnoresWhitespace()
    {
        var tokenizer = new DslTokenizer("  player   is   target  ");
        var tokens = tokenizer.Tokenize();

        Assert.HasCount(4, tokens); // player, is, target, EndOfInput
        Assert.AreEqual("player", tokens[0].Value);
        Assert.AreEqual("is", tokens[1].Value);
        Assert.AreEqual("target", tokens[2].Value);
    }

    [TestMethod]
    public void Tokenize_TrackPositions()
    {
        var tokenizer = new DslTokenizer("player is target");
        var tokens = tokenizer.Tokenize();

        Assert.AreEqual(0, tokens[0].StartIndex);
        Assert.IsGreaterThan(tokens[0].EndIndex, tokens[1].StartIndex);
        Assert.IsGreaterThan(tokens[1].EndIndex, tokens[2].StartIndex);
    }

    [TestMethod]
    public void Tokenize_CaseInsensitiveKeywords()
    {
        var tests = new[] { "AND", "And", "aNd", "and" };

        foreach (var test in tests)
        {
            var tokenizer = new DslTokenizer(test);
            var tokens = tokenizer.Tokenize();
            Assert.AreEqual(TokenType.And, tokens[0].Type, $"Failed for: {test}");
        }
    }

    [TestMethod]
    public void Tokenize_ComplexExpression()
    {
        var tokenizer = new DslTokenizer("(player.attribute constitution is_less_than 3) and not target.state is open");
        var tokens = tokenizer.Tokenize();

        Assert.IsGreaterThan(10, tokens.Count);
        Assert.AreEqual(TokenType.ParenOpen, tokens[0].Type);
        Assert.AreEqual(TokenType.IsLessThan, tokens[5].Type);
        Assert.IsTrue(tokens.Any(t => t.Type == TokenType.And));
        Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Not));
    }

    [TestMethod]
    public void Tokenize_UnknownCharacter_MarkedAsUnknown()
    {
        var tokenizer = new DslTokenizer("player @ target");
        var tokens = tokenizer.Tokenize();

        Assert.IsTrue(tokens.Any(t => t.Type == TokenType.Unknown));
    }

    [TestMethod]
    public void Tokenize_EmptyString_ReturnsOnlyEndOfInput()
    {
        var tokenizer = new DslTokenizer("");
        var tokens = tokenizer.Tokenize();

        Assert.HasCount(1, tokens);
        Assert.AreEqual(TokenType.EndOfInput, tokens[0].Type);
    }

    [TestMethod]
    public void Tokenize_MultilineInput_IgnoresNewlines()
    {
        var tokenizer = new DslTokenizer("player\nis\ntarget");
        var tokens = tokenizer.Tokenize();

        Assert.HasCount(4, tokens); // player, is, target, EndOfInput
    }
}

/// <summary>
/// Comprehensive test suite for DSL Parser.
/// </summary>
[TestClass]
public class DslParserComprehensiveTests
{
    private DslParser _parser = null!;

    [TestInitialize]
    public void Setup()
    {
        _parser = new DslParser();
    }

    [TestMethod]
    public void Parse_SimpleComparison_Succeeds()
    {
        var result = _parser.Parse("player is target");
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Ast);
        Assert.IsInstanceOfType<RelationNode>(result.Ast);
    }

    [TestMethod]
    public void Parse_TwoComparisons_WithAnd()
    {
        var result = _parser.Parse("player is target and target.state is open");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<AndNode>(result.Ast);

        var andNode = (AndNode)result.Ast!;
        Assert.IsInstanceOfType<RelationNode>(andNode.Left);
        Assert.IsInstanceOfType<RelationNode>(andNode.Right);
    }

    [TestMethod]
    public void Parse_TwoComparisons_WithOr()
    {
        var result = _parser.Parse("player is target or player is target2");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<OrNode>(result.Ast);

        var orNode = (OrNode)result.Ast!;
        Assert.IsInstanceOfType<RelationNode>(orNode.Left);
        Assert.IsInstanceOfType<RelationNode>(orNode.Right);
    }

    [TestMethod]
    public void Parse_NotExpression()
    {
        var result = _parser.Parse("not target.state is open");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<NotNode>(result.Ast);

        var notNode = (NotNode)result.Ast!;
        Assert.IsInstanceOfType<RelationNode>(notNode.Inner);
    }

    [TestMethod]
    public void Parse_DoubleNegation()
    {
        var result = _parser.Parse("not not player is target");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<NotNode>(result.Ast);

        var outerNot = (NotNode)result.Ast!;
        Assert.IsInstanceOfType<NotNode>(outerNot.Inner);
    }

    [TestMethod]
    public void Parse_Parentheses_SimpleExpression()
    {
        var result = _parser.Parse("(player is target)");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<RelationNode>(result.Ast);
    }

    [TestMethod]
    public void Parse_Parentheses_ComplexExpression()
    {
        var result = _parser.Parse("(player is target and target.state is open) or player is target2");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<OrNode>(result.Ast);
    }

    [TestMethod]
    public void Parse_AndOperator_LeftAssociativity()
    {
        // a and b and c should be parsed as (a and b) and c
        var result = _parser.Parse("player is a and player is b and player is c");
        Assert.IsTrue(result.Success);

        var outer = (AndNode)result.Ast!;
        var inner = (AndNode)outer.Left;
        Assert.IsNotNull(inner);
    }

    [TestMethod]
    public void Parse_OrOperator_LeftAssociativity()
    {
        var result = _parser.Parse("player is a or player is b or player is c");
        Assert.IsTrue(result.Success);

        var outer = (OrNode)result.Ast!;
        var inner = (OrNode)outer.Left;
        Assert.IsNotNull(inner);
    }

    [TestMethod]
    public void Parse_MixedOperators_AndHasHigherPrecedence()
    {
        // a or b and c should be parsed as a or (b and c)
        var result = _parser.Parse("player is a or player is b and player is c");
        Assert.IsTrue(result.Success);

        var outer = (OrNode)result.Ast!;
        Assert.IsInstanceOfType<AndNode>(outer.Right);
    }

    [TestMethod]
    public void Parse_IsComparisonOperator()
    {
        var result = _parser.Parse("player is target");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("is", rel.Relation);
    }

    [TestMethod]
    public void Parse_IsNotComparisonOperator()
    {
        var result = _parser.Parse("player is_not target");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("is_not", rel.Relation);
    }

    [TestMethod]
    public void Parse_NumericComparisons()
    {
        var operators = new[] { "is_less_than", "is_greater_than", "is_equal_to", "is_not_equal_to" };

        foreach (var op in operators)
        {
            var result = _parser.Parse($"player.attribute constitution {op} 3");
            Assert.IsTrue(result.Success, $"Failed for operator: {op}");
        }
    }

    [TestMethod]
    public void Parse_PropertyAccess_SingleLevel()
    {
        var result = _parser.Parse("target.state is open");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("state", rel.PropertyName);
    }

    [TestMethod]
    public void Parse_PropertyAccess_MultiLevel()
    {
        var result = _parser.Parse("target.state is open");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.IsNotNull(rel.PropertyName);
    }

    [TestMethod]
    public void Parse_Attribute_AccessKeyword()
    {
        var result = _parser.Parse("player.attribute constitution is_less_than 3");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("constitution", rel.AttributeName);
    }

    [TestMethod]
    public void Parse_SubjectTypes_Player()
    {
        var result = _parser.Parse("player is target");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("player", rel.Subject.Kind);
    }

    [TestMethod]
    public void Parse_SubjectTypes_Target()
    {
        var result = _parser.Parse("target is player");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("target", rel.Subject.Kind);
    }

    [TestMethod]
    public void Parse_SubjectTypes_Target2()
    {
        var result = _parser.Parse("target2 is player");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("target2", rel.Subject.Kind);
    }

    [TestMethod]
    public void Parse_SubjectTypes_CurrentScene()
    {
        var result = _parser.Parse("currentScene is temple");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("currentScene", rel.Subject.Kind);
    }

    [TestMethod]
    public void Parse_SubjectTypes_ItemWithId()
    {
        var result = _parser.Parse("item jade_key is_in player");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("item", rel.Subject.Kind);
        Assert.AreEqual("jade_key", rel.Subject.Id);
    }

    [TestMethod]
    public void Parse_SubjectTypes_NpcWithId()
    {
        var result = _parser.Parse("npc guard is_in kitchen");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("npc", rel.Subject.Kind);
        Assert.AreEqual("guard", rel.Subject.Id);
    }

    [TestMethod]
    public void Parse_SubjectTypes_SceneWithId()
    {
        var result = _parser.Parse("scene kitchen is player");
        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void Parse_ValueTypes_Identifier()
    {
        var result = _parser.Parse("player is target");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("element", rel.Object.Kind);
        Assert.AreEqual("target", rel.Object.Value);
    }

    [TestMethod]
    public void Parse_ValueTypes_Number()
    {
        var result = _parser.Parse("player.attribute constitution is_less_than 42");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("literal", rel.Object.Kind);
        Assert.AreEqual(42, rel.Object.NumericValue);
    }

    [TestMethod]
    public void Parse_ValueTypes_DecimalNumber()
    {
        var result = _parser.Parse("player.health is_greater_than 3.14");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.IsTrue(rel.Object.NumericValue.HasValue);
        Assert.IsGreaterThan(3.0, rel.Object.NumericValue.Value);
    }

    [TestMethod]
    public void Parse_ValueTypes_True()
    {
        var result = _parser.Parse("target.isMovable is true");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("literal", rel.Object.Kind);
        Assert.IsTrue(rel.Object.BoolValue);
    }

    [TestMethod]
    public void Parse_ValueTypes_False()
    {
        var result = _parser.Parse("target.isMovable is false");
        Assert.IsTrue(result.Success);
        var rel = (RelationNode)result.Ast!;
        Assert.AreEqual("literal", rel.Object.Kind);
        Assert.IsFalse(rel.Object.BoolValue);
    }

    [TestMethod]
    public void Parse_VisitsClause()
    {
        var result = _parser.Parse("player.visits kitchen is_greater_than 3");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<CountRelationNode>(result.Ast);

        var count = (CountRelationNode)result.Ast!;
        Assert.AreEqual("player", count.Subject.Kind);
        Assert.AreEqual("kitchen", count.SceneName);
        Assert.AreEqual("is_greater_than", count.Comparison);
        Assert.AreEqual(3, count.Value);
    }

    [TestMethod]
    public void Parse_DistanceClause()
    {
        var result = _parser.Parse("npc monster.distance_from player is_greater_than 2");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<DistanceRelationNode>(result.Ast);

        var dist = (DistanceRelationNode)result.Ast!;
        Assert.AreEqual("npc", dist.SubjectA.Kind);
        Assert.AreEqual("player", dist.SubjectB.Kind);
        Assert.AreEqual("is_greater_than", dist.Comparison);
        Assert.AreEqual(2, dist.Value);
    }

    [TestMethod]
    public void Parse_SyntaxError_MissingValue()
    {
        var result = _parser.Parse("player is");
        Assert.IsFalse(result.Success);
        Assert.IsNotEmpty(result.Errors);
    }

    [TestMethod]
    public void Parse_SyntaxError_UnexpectedToken()
    {
        var result = _parser.Parse("player is target @");
        Assert.IsFalse(result.Success);
        Assert.IsNotEmpty(result.Errors);
    }

    [TestMethod]
    public void Parse_SyntaxError_UnmatchedParenthesis()
    {
        var result = _parser.Parse("(player is target");
        Assert.IsFalse(result.Success);
        Assert.IsNotEmpty(result.Errors);
    }

    [TestMethod]
    public void Parse_ComplexExpression_FullSyntax()
    {
        var dsl = "(player has jade_key or player has silver_key) and " +
                  "not target.state is locked and " +
                  "player.visits treasury is_greater_than 0";

        var result = _parser.Parse(dsl);
        Assert.IsTrue(result.Success, string.Join("\n", result.Errors));
        Assert.IsNotNull(result.Ast);
    }

    [TestMethod]
    public void Parse_NestedParentheses()
    {
        var result = _parser.Parse("((player is target))");
        Assert.IsTrue(result.Success);
        Assert.IsInstanceOfType<RelationNode>(result.Ast);
    }

    [TestMethod]
    public void Parse_EmptyString_Fails()
    {
        var result = _parser.Parse("");
        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    public void Parse_OnlyWhitespace_Fails()
    {
        var result = _parser.Parse("   ");
        Assert.IsFalse(result.Success);
    }
}

/// <summary>
/// Comprehensive test suite for AST Node classes.
/// </summary>
[TestClass]
public class DslAstNodeTests
{
    [TestMethod]
    public void AndNode_StoresLeftAndRight()
    {
        var left = new RelationNode { Subject = new SubjectRef { Kind = "player" } };
        var right = new RelationNode { Subject = new SubjectRef { Kind = "target" } };

        var and = new AndNode { Left = left, Right = right };

        Assert.AreSame(left, and.Left);
        Assert.AreSame(right, and.Right);
    }

    [TestMethod]
    public void OrNode_StoresLeftAndRight()
    {
        var left = new RelationNode { Subject = new SubjectRef { Kind = "player" } };
        var right = new RelationNode { Subject = new SubjectRef { Kind = "target" } };

        var or = new OrNode { Left = left, Right = right };

        Assert.AreSame(left, or.Left);
        Assert.AreSame(right, or.Right);
    }

    [TestMethod]
    public void NotNode_StoresInner()
    {
        var inner = new RelationNode { Subject = new SubjectRef { Kind = "player" } };
        var not = new NotNode { Inner = inner };

        Assert.AreSame(inner, not.Inner);
    }

    [TestMethod]
    public void RelationNode_StoresAllProperties()
    {
        var subject = new SubjectRef { Kind = "player" };
        var obj = new ObjectRef { Kind = "literal", Value = "open" };

        var rel = new RelationNode
        {
            Subject = subject,
            Relation = "is",
            Object = obj,
            AttributeName = "health",
            PropertyName = "state"
        };

        Assert.AreSame(subject, rel.Subject);
        Assert.AreEqual("is", rel.Relation);
        Assert.AreSame(obj, rel.Object);
        Assert.AreEqual("health", rel.AttributeName);
        Assert.AreEqual("state", rel.PropertyName);
    }

    [TestMethod]
    public void SubjectRef_StoresKindAndId()
    {
        var subj = new SubjectRef { Kind = "item", Id = "jade_key" };

        Assert.AreEqual("item", subj.Kind);
        Assert.AreEqual("jade_key", subj.Id);
    }

    [TestMethod]
    public void ObjectRef_StoresNumericValue()
    {
        var obj = new ObjectRef { Kind = "literal", Value = "42", NumericValue = 42.0 };

        Assert.AreEqual(42.0, obj.NumericValue);
    }

    [TestMethod]
    public void ObjectRef_StoresBoolValue()
    {
        var obj = new ObjectRef { Kind = "literal", Value = "true", BoolValue = true };

        Assert.IsTrue(obj.BoolValue);
    }

    [TestMethod]
    public void CountRelationNode_StoresAllProperties()
    {
        var subj = new SubjectRef { Kind = "player" };
        var count = new CountRelationNode
        {
            Subject = subj,
            SceneName = "kitchen",
            Comparison = "is_greater_than",
            Value = 3
        };

        Assert.AreSame(subj, count.Subject);
        Assert.AreEqual("kitchen", count.SceneName);
        Assert.AreEqual("is_greater_than", count.Comparison);
        Assert.AreEqual(3, count.Value);
    }

    [TestMethod]
    public void DistanceRelationNode_StoresAllProperties()
    {
        var subjA = new SubjectRef { Kind = "npc", Id = "guard" };
        var subjB = new SubjectRef { Kind = "player" };
        var dist = new DistanceRelationNode
        {
            SubjectA = subjA,
            SubjectB = subjB,
            Comparison = "is_less_than",
            Value = 5
        };

        Assert.AreSame(subjA, dist.SubjectA);
        Assert.AreSame(subjB, dist.SubjectB);
        Assert.AreEqual("is_less_than", dist.Comparison);
        Assert.AreEqual(5, dist.Value);
    }

    [TestMethod]
    public void SubjectRef_ToString()
    {
        var subj1 = new SubjectRef { Kind = "player", Id = null };
        var subj2 = new SubjectRef { Kind = "item", Id = "key" };

        Assert.AreEqual("player", subj1.ToString());
        Assert.AreEqual("item(key)", subj2.ToString());
    }

    [TestMethod]
    public void ObjectRef_ToString()
    {
        var obj = new ObjectRef { Kind = "element", Value = "target" };
        Assert.Contains("element", obj.ToString());
        Assert.Contains("target", obj.ToString());
    }

    [TestMethod]
    public void VisitorPattern_AndNode()
    {
        var and = new AndNode { Left = new RelationNode(), Right = new RelationNode() };
        var visitor = new TestNodeVisitor();

        and.Accept(visitor);
        Assert.IsTrue(visitor.VisitedAnd);
    }

    [TestMethod]
    public void VisitorPattern_OrNode()
    {
        var or = new OrNode { Left = new RelationNode(), Right = new RelationNode() };
        var visitor = new TestNodeVisitor();

        or.Accept(visitor);
        Assert.IsTrue(visitor.VisitedOr);
    }

    [TestMethod]
    public void VisitorPattern_NotNode()
    {
        var not = new NotNode { Inner = new RelationNode() };
        var visitor = new TestNodeVisitor();

        not.Accept(visitor);
        Assert.IsTrue(visitor.VisitedNot);
    }

    // Helper visitor for testing
    private class TestNodeVisitor : INodeVisitor
    {
        public bool VisitedAnd { get; set; }
        public bool VisitedOr { get; set; }
        public bool VisitedNot { get; set; }

        public string Visit(ConditionNode node) => "unknown";
        public string Visit(AndNode node) { VisitedAnd = true; return "and"; }
        public string Visit(OrNode node) { VisitedOr = true; return "or"; }
        public string Visit(NotNode node) { VisitedNot = true; return "not"; }
        public string Visit(RelationNode node) => "relation";
        public string Visit(CountRelationNode node) => "count";
        public string Visit(DistanceRelationNode node) => "distance";
    }
}
