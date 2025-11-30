namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Describes a parameter that a condition operator requires.
/// Used by the editor to generate appropriate UI controls and validate input.
/// </summary>
public sealed class ConditionParameterDescriptor
{
    /// <summary>
    /// Parameter name (used as the key in the parameters dictionary)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The type/kind of this parameter (determines UI rendering)
    /// </summary>
    public ConditionParameterKind Kind { get; set; }
    
    /// <summary>
    /// Whether this parameter is required for the condition to evaluate
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// Human-readable description shown in the editor UI
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Optional default value for the parameter
    /// </summary>
    public string? DefaultValue { get; set; }
}
