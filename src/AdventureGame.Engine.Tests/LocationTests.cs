// ============================================================================
// AdventureGame.Engine.Tests/LocationTests.cs
// ============================================================================
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Tests;

[TestClass]
public class LocationTests
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = false // compact for assertions
    };

    [TestMethod]
    public void Location_World_RoundTrip()
    {
        var loc = Location.World(new GridPosition(2, -3, 1));
        var json = JsonSerializer.Serialize(loc, JsonOpts);
        var back = JsonSerializer.Deserialize<Location>(json, JsonOpts)!;

        Assert.IsTrue(back.IsWorld);
        Assert.IsTrue(back.TryGetPosition(out var p));
        Assert.AreEqual(2, p.X);
        Assert.AreEqual(-3, p.Y);
        Assert.AreEqual(1, p.Z);
    }

    [TestMethod]
    public void Location_Embedded_RoundTrip()
    {
        var parent = ElementId.New();
        var loc = Location.Embedded(parent);
        var json = JsonSerializer.Serialize(loc, JsonOpts);
        var back = JsonSerializer.Deserialize<Location>(json, JsonOpts)!;

        Assert.IsTrue(back.IsEmbedded);
        Assert.IsTrue(back.TryGetParent(out var pid));
        Assert.AreEqual(parent.Value, pid.Value);
    }

    [TestMethod]
    public void Location_Special_RoundTrip()
    {
        var loc = Location.SpecialOf(SpecialPlace.Inventory);
        var json = JsonSerializer.Serialize(loc, JsonOpts);
        var back = JsonSerializer.Deserialize<Location>(json, JsonOpts)!;

        Assert.IsTrue(back.IsSpecial);
        Assert.IsTrue(back.TryGetSpecial(out var sp));
        Assert.AreEqual(SpecialPlace.Inventory, sp);
    }
}
