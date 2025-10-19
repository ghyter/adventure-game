// ============================================================================
// AdventureGame.Engine.Tests/ExitValidationTests.cs
// ============================================================================
namespace AdventureGame.Engine.Tests;

[TestClass]
public class ExitValidationTests
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

    private static Exit MakeExitIn(Scene host, ExitMode mode, Direction? dir = null)
    {
        var ex = new Exit
        {
            Name = "Exit",
            Description = "Desc",
            Location = Location.Embedded(host.Id),
            Mode = mode,
            Direction = dir
        };
        ex.States["default"] = "d";
        ex.DefaultState = "default";
        return ex;
    }

    private static Player MakePlayer()
    {
        var p = new Player { Name = "P", Description = "Hero", Location = Location.OffMap() };
        p.States["default"] = "d";
        p.DefaultState = "default";
        return p;
    }

    [TestMethod]
    public void Exit_Directional_Autowires_ToAdjacentScene()
    {
        var pack = new GamePack { Name = "Autowire", Grid = new GridConfig { Entry = new GridPosition(0, 0, 0) } };

        var a = MakeScene("A", new GridPosition(0, 0, 0), new Dimensions(1, 1, 1));
        var b = MakeScene("B", new GridPosition(1, 0, 0), new Dimensions(1, 1, 1)); // east of A
        var ex = MakeExitIn(a, ExitMode.Directional, Direction.East);

        pack.Elements.AddRange(new GameElement[] { a, b, ex, MakePlayer() });

        pack.ValidateOrThrow();

        // Expect autowired target to B.Id
        Assert.AreEqual(1, ex.Targets.Count);
        Assert.AreEqual(b.Id.Value, ex.Targets.First().Value);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Exit_Directional_Ambiguous_WhenMultipleNeighborsInBlock()
    {
        var pack = new GamePack { Name = "Ambiguous", Grid = new GridConfig { Entry = new GridPosition(0, 0, 0) } };

        // Host scene spans 2x2 so NE adjacent block is 2x2 (four cells)
        var host = MakeScene("Host", new GridPosition(0, 0, 0), new Dimensions(2, 2, 1));
        var s1 = MakeScene("S1", new GridPosition(1, 1, 0), new Dimensions(1, 1, 1)); // in NE block
        var s2 = MakeScene("S2", new GridPosition(2, 2, 0), new Dimensions(1, 1, 1)); // also in NE block (diagonally away)

        var ex = MakeExitIn(host, ExitMode.Directional, Direction.NorthEast);

        pack.Elements.AddRange(new GameElement[] { host, s1, s2, ex, MakePlayer() });

        // Should throw: ambiguity (two different scenes adjacent in the NE block)
        pack.ValidateOrThrow();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Exit_Custom_Requires_AtLeastOneTarget()
    {
        var pack = new GamePack { Name = "CustomExit", Grid = new GridConfig { Entry = new GridPosition(0, 0, 0) } };

        var host = MakeScene("Host", new GridPosition(0, 0, 0), new Dimensions(1, 1, 1));
        var ex = MakeExitIn(host, ExitMode.Custom);
        // no targets set

        pack.Elements.AddRange(new GameElement[] { host, ex, MakePlayer() });

        pack.ValidateOrThrow(); // should throw: custom requires target
    }

    [TestMethod]
    public void Exit_Portal_WithTarget_IsValid()
    {
        var pack = new GamePack { Name = "Portal", Grid = new GridConfig { Entry = new GridPosition(0, 0, 0) } };

        var a = MakeScene("A", new GridPosition(0, 0, 0), new Dimensions(1, 1, 1));
        var b = MakeScene("B", new GridPosition(5, 5, 0), new Dimensions(1, 1, 1));
        var ex = MakeExitIn(a, ExitMode.Portal);
        ex.Targets.Add(b.Id);

        pack.Elements.AddRange(new GameElement[] { a, b, ex, MakePlayer() });

        pack.ValidateOrThrow(); // should pass
        Assert.AreEqual(1, ex.Targets.Count);

        Console.WriteLine(pack.ToJson());

        Assert.Fail("Checking log");


    }
}