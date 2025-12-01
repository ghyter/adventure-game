namespace AdventureGame.Engine.Verbs;

using AdventureGame.Engine.Filters;

public class Verb
{
    public string Name { get; set; } = "";
    public HashSet<string> Aliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Number of target arguments this verb requires (0, 1, or 2).
    /// Determines which filters are actually used when resolving the verb.
    /// </summary>
    public int TargetCount { get; set; } = 0;

    public GameElementFilter Target1 { get; set; } = new() { Mode = GameElementFilterMode.None };
    public GameElementFilter Target2 { get; set; } = new() { Mode = GameElementFilterMode.None };

    public List<string> ConditionTexts { get; set; } = [];
    public List<VerbEffect> Effects { get; set; } = [];
}
