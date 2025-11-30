namespace AdventureGame.Engine.Actions;

/// <summary>
/// Defines the type of parameter that an effect action requires.
/// Used by the editor UI to render appropriate input controls.
/// </summary>
public enum EffectParameterKind
{
    /// <summary>
    /// A reference to a game element (Target1, Target2, or a specific element ID)
    /// </summary>
    Target,
    
    /// <summary>
    /// Name of a property on a game element (e.g., "Health", "IsVisible")
    /// </summary>
    PropertyName,
    
    /// <summary>
    /// Name of an attribute in the element's Attributes dictionary
    /// </summary>
    AttributeName,
    
    /// <summary>
    /// Free-form string value
    /// </summary>
    Value,
    
    /// <summary>
    /// Numeric value (integer or decimal)
    /// </summary>
    Number,
    
    /// <summary>
    /// Boolean value (true/false)
    /// </summary>
    Boolean,
    
    /// <summary>
    /// Location reference (Scene ID or coordinates)
    /// </summary>
    Location,
    
    /// <summary>
    /// Exit reference (Exit element ID or direction)
    /// </summary>
    Exit,
    
    /// <summary>
    /// Group or collection identifier
    /// </summary>
    Group,
    
    /// <summary>
    /// Dice expression (e.g., "2d6+3", "1d20", "3d6+1d4")
    /// </summary>
    DiceExpression
}
