namespace AdventureGame.Engine.Parameters;

/// <summary>
/// Describes a parameter used by both Conditions and Effects.
/// Replaces EffectParameterDescriptor and ConditionParameterDescriptor.
/// </summary>
public sealed class ParameterDescriptor
{
    /// <summary>
    /// Parameter name (used as the key in the parameters dictionary)
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable name shown in the editor UI
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// The type key that references an IParameterTypeHandler
    /// (e.g., "gameElement", "number", "boolean", "diceExpression")
    /// </summary>
    public string ParameterType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this parameter is required (if false, it's optional)
    /// </summary>
    public bool IsOptional { get; set; }
    
    /// <summary>
    /// Optional description shown in the editor UI
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Optional default value for the parameter
    /// </summary>
    public string? DefaultValue { get; set; }
}
