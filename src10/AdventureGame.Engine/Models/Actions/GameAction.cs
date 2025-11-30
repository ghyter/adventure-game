using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models.Actions;

/// <summary>
/// Unified model for both player-initiated verbs and automatic triggers.
/// Replaces separate Verb and Trigger models with a single extensible structure.
/// </summary>
public sealed class GameAction
{
    /// <summary>
    /// Unique identifier for this action
    /// </summary>
    [JsonInclude]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Distinguishes between verbs (player-initiated) and triggers (automatic)
    /// </summary>
    [JsonInclude]
    public ActionType Type { get; set; }

    /// <summary>
    /// Human-friendly name for listing in the editor.
    /// For verbs this can be the verb phrase.
    /// For triggers this should be a descriptive label.
    /// </summary>
    [JsonInclude]
    public string? Name { get; set; }

    // ========== VERB-ONLY FIELDS ==========
    
    /// <summary>
    /// Canonical verb phrase (e.g., "open", "push", "attack").
    /// Only valid when Type == ActionType.Verb.
    /// Null or ignored for triggers.
    /// </summary>
    [JsonInclude]
    public string? VerbPhrase { get; set; }

    /// <summary>
    /// Number of targets this action expects from the parsed command: 0, 1, or 2.
    /// Only meaningful when Type == ActionType.Verb.
    /// For triggers this must be 0.
    /// </summary>
    [JsonInclude]
    public int TargetCount { get; set; }

    // ========== ALIASES & TAGS (like GameElement) ==========
    
    /// <summary>
    /// Alternative names or synonyms for this action.
    /// For verbs: "grab", "pick up" might be aliases for "take"
    /// For triggers: alternative trigger names
    /// Case-insensitive HashSet for uniqueness.
    /// </summary>
    [JsonInclude]
    public HashSet<string> Aliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Tags for categorizing and organizing actions.
    /// Examples: "movement", "combat", "interaction", "puzzle"
    /// Case-insensitive HashSet for uniqueness.
    /// </summary>
    [JsonInclude]
    public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // ========== OPTIONAL SKILL CHECK ==========
    
    /// <summary>
    /// Optional skill check definition (dice roll vs difficulty)
    /// </summary>
    [JsonInclude]
    public SkillCheckDefinition? SkillCheck { get; set; }

    // ========== CONDITION / EFFECT GROUPS ==========
    
    /// <summary>
    /// Condition groups that must be satisfied for this action to execute.
    /// Multiple groups are combined with AND logic.
    /// Each group internally uses its own operator (And/Or).
    /// </summary>
    [JsonInclude]
    public List<ConditionGroup> ConditionGroups { get; set; } = [];

    /// <summary>
    /// Effect groups to execute when this action fires.
    /// Executed in order after conditions pass.
    /// </summary>
    [JsonInclude]
    public List<EffectGroup> EffectGroups { get; set; } = [];

    // ========== METADATA ==========
    
    /// <summary>
    /// Optional notes/description to help authors understand this action.
    /// Not used by runtime execution.
    /// </summary>
    [JsonInclude]
    public string? Notes { get; set; }
    
    /// <summary>
    /// When this action was created (for editor sorting/display)
    /// </summary>
    [JsonInclude]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this action was last modified (for editor sorting/display)
    /// </summary>
    [JsonInclude]
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
