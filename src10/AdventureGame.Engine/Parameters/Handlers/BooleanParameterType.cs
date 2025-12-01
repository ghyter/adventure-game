namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for boolean (true/false) values.
/// </summary>
public sealed class BooleanParameterType : IParameterTypeHandler
{
    public string Key => "boolean";
    
    public string DisplayName => "Boolean";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_Boolean";
    
    public object? Deserialize(object? rawValue)
    {
        if (rawValue is bool b) return b;
        if (bool.TryParse(rawValue?.ToString(), out var parsed))
            return parsed;
        return false;
    }
}
