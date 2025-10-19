// ============================================================================
// AdventureGame.Engine.Tests/PlayerAndSessionTests.cs
// ============================================================================
namespace AdventureGame.Engine.Tests;

[TestClass]
public class PlayerAndSessionTests
{
    private static Scene MakeEntry()
    {
        var s = new Scene
        {
            Name = "Entry",
            Description = "Entry scene",
            Location = Location.World(new GridPosition(0, 0, 0)),
            ExtentInCells = new Dimensions(1, 1, 1)
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
    public void PlayerCandidates_And_Session_AutoSelect_Single()
    {
        var pack = new GamePack { Name = "Game", Grid = new GridConfig { Entry = GridPosition.Origin } };
        pack.Elements.Add(MakeEntry());
        var only = MakePlayer("Only");
        pack.Elements.Add(only);

        pack.ValidateOrThrow();

        var session = GameSession.FromPack(pack, autoSelectSinglePlayer: true);
        Assert.IsNotNull(session.ActivePlayerId);
        Assert.AreEqual(only.Id.Value, session.ActivePlayerId!.Value.Value);
        Assert.IsNotNull(session.ActivePlayerSe);
    }

    [TestMethod]
    public void PlayerCandidates_NoAutoSelect_WhenMultiple()
    {
        var pack = new GamePack { Name = "Game", Grid = new GridConfig { Entry = GridPosition.Origin } };
        pack.Elements.Add(MakeEntry());
        var p1 = MakePlayer("A");
        var p2 = MakePlayer("B");
        pack.Elements.AddRange(new GameElement[] { p1, p2 });

        pack.ValidateOrThrow();

        var session = GameSession.FromPack(pack, autoSelectSinglePlayer: true);
        Assert.IsNull(session.ActivePlayerId); // multiple candidates -> no auto select
    }
}