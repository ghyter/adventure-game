namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for numeric (integer) values.
/// </summary>
public sealed class NumberParameterType : IParameterTypeHandler
{
    public string Key => "number";
    
    public string DisplayName => "Number";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_Number";
    
    public object? Deserialize(object? rawValue)
    {
        if (rawValue is int i) return i;
        if (int.TryParse(rawValue?.ToString(), out var parsed))
            return parsed;
        return 0;
    }
}
