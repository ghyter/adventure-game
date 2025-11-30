namespace AdventureGame.Engine.Actions;

/// <summary>
/// Describes a parameter that an effect action requires.
/// Used by the editor to generate appropriate UI controls and validate input.
/// </summary>
public sealed class EffectParameterDescriptor
{
    /// <summary>
    /// Parameter name (used as the key in the parameters dictionary)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The type/kind of this parameter (determines UI rendering)
    /// </summary>
    public EffectParameterKind Kind { get; set; }
    
    /// <summary>
    /// Whether this parameter is required for the effect to execute
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
