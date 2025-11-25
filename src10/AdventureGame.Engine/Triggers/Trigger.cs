namespace AdventureGame.Engine.Triggers;

using AdventureGame.Engine.Verbs;

public class Trigger
{
    public string Name { get; set; } = "";
    public List<string> ConditionTexts { get; set; } = new();
    public List<VerbEffect> Effects { get; set; } = new();

    public bool FiredThisRound { get; set; } = false;
}
