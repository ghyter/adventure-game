// ============================================================================
// AdventureGame.Engine.Tests/GamePackValidationTests.cs
// ============================================================================
namespace AdventureGame.Engine.Tests;

[TestClass]
public class GamePackValidationTests
{
    private static Scene MakeScene(string name, GridPosition pos, Dimensions span)
    {
        var s = new Scene
        {
            Name = name,
            Description = "Desc",
            Location = Location.World(pos),
            ExtentInCells = span
        };
        s.States["default"] = "d";
        s.DefaultState = "default";
        return s;
    }

    private static Player MakePlayer(string name)
    {
        var p = new Player { Name = name, Description = "Hero", Location = Location.OffMap() };
        p.States["default"] = "d";
        p.DefaultState = "default";
        return p;
    }

    [TestMethod]
    public void GamePack_Validate_Passes_WhenEntryOccupied()
    {
        var pack = new GamePack { Name = "Test", Grid = new GridConfig { Entry = new GridPosition(0, 0, 0) } };

        var entry = MakeScene("Entry", new GridPosition(0, 0, 0), new Dimensions(1, 1, 1));
        pack.Elements.Add(entry);
        pack.Elements.Add(MakePlayer("P1"));

        pack.ValidateOrThrow();
        Assert.IsTrue(pack.TryGetSceneAt(new GridPosition(0, 0, 0), out var s) && s == entry);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GamePack_Validate_Fails_OnOverlap()
    {
        var pack = new GamePack { Name = "Overlap", Grid = new GridConfig { Entry = GridPosition.Origin } };

        // Two 1x1x1 scenes at same cell
        pack.Elements.Add(MakeScene("A", GridPosition.Origin, new Dimensions(1, 1, 1)));
        pack.Elements.Add(MakeScene("B", GridPosition.Origin, new Dimensions(1, 1, 1)));
        pack.Elements.Add(MakePlayer("P1"));

        pack.ValidateOrThrow(); // should throw overlap
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GamePack_Validate_Fails_EmbeddedParentMissing()
    {
        var pack = new GamePack { Name = "EmbeddedMissing", Grid = new GridConfig { Entry = new GridPosition(0, 0, 0) } };

        // World scene for entry
        pack.Elements.Add(MakeScene("Entry", new GridPosition(0, 0, 0), new Dimensions(1, 1, 1)));
        pack.Elements.Add(MakePlayer("P1"));

        // Embedded scene with bad parent
        var child = MakeScene("Workbench", new GridPosition(99, 99, 99), new Dimensions(1, 1, 1));
        child.Location = Location.Embedded(ElementId.New()); // parent doesn't exist in pack
        pack.Elements.Add(child);

        pack.ValidateOrThrow(); // should throw
    }
}
