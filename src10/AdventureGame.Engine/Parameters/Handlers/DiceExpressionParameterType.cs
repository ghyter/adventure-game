namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for dice expressions (e.g., "2d6+3", "1d20").
/// </summary>
public sealed class DiceExpressionParameterType : IParameterTypeHandler
{
    public string Key => "diceExpression";
    
    public string DisplayName => "Dice Expression";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_DiceExpression";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
