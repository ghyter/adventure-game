namespace AdventureGame.Engine.Parser;

using AdventureGame.Engine.Models;
using AdventureGame.Engine.Runtime;
using AdventureGame.Engine.Models.Elements;

public class CommandParser
{
    private static readonly string[] FluffWords = 
    { 
        "the", "a", "an", "to", "at", "with", "using", "on", "in", "from" 
    };

    private static readonly Dictionary<string, Direction> DirectionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["north"] = Direction.North,
        ["n"] = Direction.North,
        ["south"] = Direction.South,
        ["s"] = Direction.South,
        ["east"] = Direction.East,
        ["e"] = Direction.East,
        ["west"] = Direction.West,
        ["w"] = Direction.West,
        ["northeast"] = Direction.NorthEast,
        ["ne"] = Direction.NorthEast,
        ["northwest"] = Direction.NorthWest,
        ["nw"] = Direction.NorthWest,
        ["southeast"] = Direction.SouthEast,
        ["se"] = Direction.SouthEast,
        ["southwest"] = Direction.SouthWest,
        ["sw"] = Direction.SouthWest,
        ["up"] = Direction.Up,
        ["u"] = Direction.Up,
        ["down"] = Direction.Down,
        ["d"] = Direction.Down
    };

    public ParsedCommand Parse(string input, GameSession session)
    {
        var result = new ParsedCommand { RawInput = input };

        if (string.IsNullOrWhiteSpace(input))
            return result;

        // Split and remove fluff words
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Where(t => !FluffWords.Contains(t, StringComparer.OrdinalIgnoreCase))
                          .ToList();

        if (tokens.Count == 0)
            return result;

        // First token is the verb
        result.VerbToken = tokens[0];

        // Try to resolve targets
        if (tokens.Count > 1)
        {
            result.Target1 = ResolveTarget(tokens[1], session);
        }

        if (tokens.Count > 2)
        {
            result.Target2 = ResolveTarget(tokens[2], session);
        }

        return result;
    }

    private GameElement? ResolveTarget(string token, GameSession session)
    {
        // Check if it's a direction
        if (DirectionMap.TryGetValue(token, out var direction))
        {
            // Find exit in current scene with this direction
            if (session.CurrentScene != null)
            {
                return session.Elements
                    .OfType<Exit>()
                    .FirstOrDefault(e => 
                        e.ParentId == session.CurrentScene.Id && 
                        e.Direction == direction);
            }
        }

        // Try to find by name or alias
        return session.Elements.FirstOrDefault(e =>
            e.Name.Equals(token, StringComparison.OrdinalIgnoreCase) ||
            e.Aliases.Contains(token, StringComparer.OrdinalIgnoreCase));
    }
}

public class ParsedCommand
{
    public string RawInput { get; set; } = "";
    public string? VerbToken { get; set; }
    public GameElement? Target1 { get; set; }
    public GameElement? Target2 { get; set; }
}
