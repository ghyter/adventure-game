namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for property names on game elements.
/// </summary>
public sealed class PropertyNameParameterType : IParameterTypeHandler
{
    public string Key => "propertyName";
    
    public string DisplayName => "Property Name";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_PropertyName";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
