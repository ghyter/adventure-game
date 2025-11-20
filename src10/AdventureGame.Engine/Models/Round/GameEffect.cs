namespace AdventureGame.Engine.Models.Round;

// Models/GameEffect.cs
public sealed class GameEffect
{
    /// <summary>
    /// The primary field: Natural language DSL effect text.
    /// This is the main field that should be used.
    /// </summary>
    public string EffectText { get; set; } = "";

    /// <summary>
    /// Legacy fields, kept for migration purposes only.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? TargetId { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Action { get; set; }
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; set; }
}
