namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for string values.
/// </summary>
public sealed class StringParameterType : IParameterTypeHandler
{
    public string Key => "string";
    
    public string DisplayName => "Text";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_String";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
