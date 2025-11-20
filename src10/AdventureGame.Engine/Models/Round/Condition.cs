namespace AdventureGame.Engine.Models.Round;

// Models/Condition.cs
public sealed class GameCondition
{
    /// <summary>
    /// The primary field: Natural language DSL condition text.
    /// This is the main field that should be used.
    /// </summary>
    public string ConditionText { get; set; } = "";

    /// <summary>
    /// Legacy fields, kept for migration purposes only.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? GameElementId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Rule { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Comparison { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }
}
