namespace AdventureGame.Components.Rules;

using AdventureGame.Engine.DSL.Parser;

/// <summary>
/// Result from ConditionInput component containing the parsed condition.
/// </summary>
public class ConditionInputResult
{
    public string ConditionText { get; set; } = string.Empty;
    public DslParseResult? ParseResult { get; set; }
}
