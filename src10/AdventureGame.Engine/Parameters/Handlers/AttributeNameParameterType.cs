namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for attribute names in game element dictionaries.
/// </summary>
public sealed class AttributeNameParameterType : IParameterTypeHandler
{
    public string Key => "attributeName";
    
    public string DisplayName => "Attribute Name";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_AttributeName";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
