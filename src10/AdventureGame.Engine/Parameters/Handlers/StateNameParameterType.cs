namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for state names on game elements.
/// </summary>
public sealed class StateNameParameterType : IParameterTypeHandler
{
    public string Key => "stateName";
    
    public string DisplayName => "State Name";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_StateName";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
