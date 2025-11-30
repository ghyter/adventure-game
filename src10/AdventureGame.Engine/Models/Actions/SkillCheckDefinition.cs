using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Defines an optional skill check for an action.
/// Used for ability checks, saving throws, etc.
/// </summary>
public sealed class SkillCheckDefinition
{
    /// <summary>
    /// Identifier of the skill being checked (e.g., "lockpicking", "strength")
    /// </summary>
    [JsonInclude]
    public string? SkillId { get; set; }
    
    /// <summary>
    /// Human-readable description of the check
    /// </summary>
    [JsonInclude]
    public string? Description { get; set; }
    
    /// <summary>
    /// Difficulty class (DC) that must be met or exceeded
    /// </summary>
    [JsonInclude]
    public int Difficulty { get; set; }
    
    /// <summary>
    /// Dice expression for the check (e.g., "1d20+{skill}", "2d6+3")
    /// Placeholders like {skill} can be replaced at runtime
    /// </summary>
    [JsonInclude]
    public string? DiceExpression { get; set; }
}
