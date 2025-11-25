namespace AdventureGame.Engine.Verbs;

public class VerbEffect
{
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 0;
    public string SuccessText { get; set; } = "";
    public string FailureText { get; set; } = "";
    public string Action { get; set; } = "";
}
