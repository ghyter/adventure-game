// ============================================================================
// AdventureGame.Engine.Tests/GridAndSceneTests.cs
// ============================================================================
namespace AdventureGame.Engine.Tests;

[TestClass]
public class GridAndSceneTests
{
    private static Scene MakeScene(GridPosition pos, Dimensions span)
    {
        var s = new Scene
        {
            Name = "S",
            Description = "Desc",
            Location = Location.World(pos),
            ExtentInCells = span
        };
        s.States["default"] = "d";
        s.DefaultState = "default";
        return s;
    }

    [TestMethod]
    public void Scene_OccupiedCells_2x2x2_AtOrigin()
    {
        var s = MakeScene(new GridPosition(0, 0, 0), new Dimensions(2, 2, 2));
        var cells = s.OccupiedCells().ToHashSet();

        // Expect 8 cells (0/1 in each axis)
        Assert.AreEqual(8, cells.Count);
        var expected = new[]
        {
            new GridPosition(0,0,0), new GridPosition(1,0,0),
            new GridPosition(0,1,0), new GridPosition(1,1,0),
            new GridPosition(0,0,1), new GridPosition(1,0,1),
            new GridPosition(0,1,1), new GridPosition(1,1,1),
        };
        foreach (var c in expected)
            Assert.IsTrue(cells.Contains(c), $"Missing {c}");
    }

    [TestMethod]
    public void GridNav_Delta_Basics()
    {
        Assert.AreEqual(new GridPosition(0, 1, 0), GridNav.Delta(Direction.North));
        Assert.AreEqual(new GridPosition(1, 0, 0), GridNav.Delta(Direction.East));
        Assert.AreEqual(new GridPosition(0, 0, 1), GridNav.Delta(Direction.Up));
        Assert.AreEqual(new GridPosition(-1, -1, 0), GridNav.Delta(Direction.SouthWest));
    }
}