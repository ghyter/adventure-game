namespace AdventureGame.Engine.Tests.DSL;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdventureGame.Engine.DSL;
using AdventureGame.Engine.DSL.AST.Effects;

/// <summary>
/// Tests for Effects DSL parsing and execution.
/// </summary>
[TestClass]
public class EffectDslParserTests
{
    [TestMethod]
    public void Parse_SetStateEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "set target's state to open";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<SetStateEffect>(result);
        var effect = (SetStateEffect)result;
        Assert.AreEqual("open", effect.StateName);
        Assert.IsNotNull(effect.Subject);
        Assert.AreEqual("target", effect.Subject.Kind);
    }

    [TestMethod]
    public void Parse_SetFlagEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "set target's flag isLocked to true";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<FlagEffect>(result);
        var effect = (FlagEffect)result;
        Assert.AreEqual("isLocked", effect.FlagName);
        Assert.IsTrue(effect.Value);
    }

    [TestMethod]
    public void Parse_SetAttributeEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "set player's attribute health to 50";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<AttributeEffect>(result);
        var effect = (AttributeEffect)result;
        Assert.AreEqual("health", effect.AttributeName);
        Assert.AreEqual("set", effect.Operation);
        Assert.AreEqual(50, effect.Value);
    }

    [TestMethod]
    public void Parse_IncreaseAttributeEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "increase player's attribute health by 10";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<AttributeEffect>(result);
        var effect = (AttributeEffect)result;
        Assert.AreEqual("health", effect.AttributeName);
        Assert.AreEqual("increase", effect.Operation);
        Assert.AreEqual(10, effect.Value);
    }

    [TestMethod]
    public void Parse_DecreaseAttributeEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "decrease npc monster's attribute health by 5";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<AttributeEffect>(result);
        var effect = (AttributeEffect)result;
        Assert.AreEqual("health", effect.AttributeName);
        Assert.AreEqual("decrease", effect.Operation);
        Assert.AreEqual(5, effect.Value);
    }

    [TestMethod]
    public void Parse_MoveEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "move target to scene treasure_room";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<MoveEffect>(result);
        var effect = (MoveEffect)result;
        Assert.IsNotNull(effect.Subject);
        Assert.IsNotNull(effect.Target);
        Assert.AreEqual("target", effect.Subject.Kind);
        Assert.AreEqual("scene", effect.Target.Kind);
    }

    [TestMethod]
    public void Parse_GiveInventoryEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "give item gold_key to player";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<InventoryEffect>(result);
        var effect = (InventoryEffect)result;
        Assert.AreEqual("give", effect.Operation);
        Assert.IsNotNull(effect.Item);
        Assert.IsNotNull(effect.Subject);
    }

    [TestMethod]
    public void Parse_AddInventoryEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "add item sword to npc warrior";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<InventoryEffect>(result);
        var effect = (InventoryEffect)result;
        Assert.AreEqual("add", effect.Operation);
    }

    [TestMethod]
    public void Parse_RemoveInventoryEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "remove item cursed_amulet from player";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<InventoryEffect>(result);
        var effect = (InventoryEffect)result;
        Assert.AreEqual("remove", effect.Operation);
    }

    [TestMethod]
    public void Parse_SayEffect()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "say \"You have opened the treasure chest!\"";

        // Act
        var result = parser.Parse(input);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<SayEffect>(result);
        var effect = (SayEffect)result;
        Assert.AreEqual("You have opened the treasure chest!", effect.Message);
    }

    [TestMethod]
    public void ParseMultiple_ParsesSeveralEffects()
    {
        // Arrange
        var parser = new EffectDslParser();
        var input = "set target's state to open. give item gold_key to player. say \"Treasure unlocked!\"";

        // Act
        var results = parser.ParseMultiple(input);

        // Assert
#pragma warning disable MSTEST0037
        Assert.AreEqual(3, results.Count);
#pragma warning restore MSTEST0037
        Assert.IsInstanceOfType<SetStateEffect>(results[0]);
        Assert.IsInstanceOfType<InventoryEffect>(results[1]);
        Assert.IsInstanceOfType<SayEffect>(results[2]);
    }

    [TestMethod]
    public void Parse_HandlesNullInput()
    {
        // Arrange
        var parser = new EffectDslParser();

        // Act
        var result = parser.Parse(null!);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void Parse_HandlesEmptyInput()
    {
        // Arrange
        var parser = new EffectDslParser();

        // Act
        var result = parser.Parse("");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ParseMultiple_HandlesNullInput()
    {
        // Arrange
        var parser = new EffectDslParser();

        // Act
        var results = parser.ParseMultiple(null!);

        // Assert
#pragma warning disable MSTEST0037
        Assert.AreEqual(0, results.Count);
#pragma warning restore MSTEST0037
    }
}
