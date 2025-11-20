namespace AdventureGame.Engine.Tests.DSL;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdventureGame.Engine.DSL;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Elements;

/// <summary>
/// Tests for DSL canonicalization.
/// </summary>
[TestClass]
public class DslCanonicalizerTests
{
    [TestMethod]
    public void Canonicalize_RemovesDeterminers()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "the player is the target";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.AreEqual("player is target", result);
    }

    [TestMethod]
    public void Canonicalize_HandlesPossessives()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "player's state is open";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.AreEqual("player.state is open", result);
    }

    [TestMethod]
    public void Canonicalize_HandlesOfConstruction()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "state of the target is closed";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("target.state", result);
    }

    [TestMethod]
    public void Canonicalize_ReplacesOperatorPhrases()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "player's attribute constitution is less than 3";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("is_less_than", result);
    }

    [TestMethod]
    public void Canonicalize_ReplacesMultipleOperatorPhrases()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "npc monster.health is greater than 50 and player.attribute strength is not equal to 5";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("is_greater_than", result);
        Assert.Contains("is_not_equal_to", result);
    }

    [TestMethod]
    public void Canonicalize_ReplacesPhraseToCanonical()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        
        // Build vocabulary with element mappings
        var pack = new GamePack();
        var jadeKey = new Item { Name = "jade key" };
        var billiardRoom = new Scene { Name = "billiard room" };
        pack.Elements.Add(jadeKey);
        pack.Elements.Add(billiardRoom);
        
        var vocab = DslVocabulary.FromGamePack(pack);
        
        var input = "the jade key is in the billiard room";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("jade_key", result);
        Assert.Contains("billiard_room", result);
    }

    [TestMethod]
    public void Canonicalize_ComplexExpression()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var input = "the player's attribute constitution is less than 3 and the target's state is open";

        // Act
        var vocab = new DslVocabulary();
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.AreEqual("player.attribute constitution is_less_than 3 and target.state is open", result);
    }

    [TestMethod]
    public void Canonicalize_DistanceFromOperator()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "npc monster distance from player is greater than 2";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("distance_from", result);
    }

    [TestMethod]
    public void Canonicalize_IsInOperator()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "item gold_key is in player";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("is_in", result);
    }

    [TestMethod]
    public void Canonicalize_IsEqualToOperator()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var input = "player's level is equal to 5";

        // Act
        var result = canonicalizer.Canonicalize(input, vocab);

        // Assert
        Assert.Contains("is_equal_to", result);
    }

    [TestMethod]
    public void Canonicalize_PreservesKnownSubjects()
    {
        // Arrange
        var canonicalizer = new DslCanonicalizer();
        var vocab = new DslVocabulary();
        var subjects = new[] { "player", "target", "target2", "scene", "item", "npc", "exit", "currentscene" };

        foreach (var subject in subjects)
        {
            // Act
            var result = canonicalizer.Canonicalize($"the {subject} is target", vocab);

            // Assert
            Assert.Contains(subject, result);
        }
    }
}
