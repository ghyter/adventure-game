using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Effects;

/// <summary>
/// Represents a single effect that uses an action and parameters.
/// This is the atomic unit of effect execution.
/// </summary>
public sealed class EffectDefinition
{
    /// <summary>
    /// The key of the effect action to execute (e.g., "setProperty", "move", "rollDice")
    /// </summary>
    [JsonInclude]
    public string ActionKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Parameters specific to the action
    /// </summary>
    [JsonInclude]
    public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
