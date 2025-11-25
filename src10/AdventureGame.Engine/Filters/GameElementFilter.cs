namespace AdventureGame.Engine.Filters;

using AdventureGame.Engine.Models;

public enum GameElementFilterMode
{
    None,   // New: no target required
    All,
    Types,
    Tags,
    Names
}

public class GameElementFilter
{
    public GameElementFilterMode Mode { get; set; } = GameElementFilterMode.None;
    public List<string> Types { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Names { get; set; } = new();

    public bool Matches(GameElement element)
    {
        switch (Mode)
        {
            case GameElementFilterMode.None:
                return false; // No match expected when mode is None
            case GameElementFilterMode.All:
                return true;
            case GameElementFilterMode.Types:
                return Types.Contains(element.Kind);
            case GameElementFilterMode.Tags:
                return element.Tags.Any(t => Tags.Contains(t));
            case GameElementFilterMode.Names:
                return Names.Contains(element.Name, StringComparer.OrdinalIgnoreCase)
                    || element.Aliases.Any(a => Names.Contains(a, StringComparer.OrdinalIgnoreCase));
            default:
                return false;
        }
    }

    public int Score(GameElement element)
    {
        // If mode is None, no match is expected, return 0
        if (Mode == GameElementFilterMode.None)
            return 0;

        if (!Matches(element)) return 0;

        return Mode switch
        {
            GameElementFilterMode.All => 10,
            GameElementFilterMode.Types => 50,
            GameElementFilterMode.Tags => 80,
            GameElementFilterMode.Names => 100,
            _ => 0
        };
    }
}
