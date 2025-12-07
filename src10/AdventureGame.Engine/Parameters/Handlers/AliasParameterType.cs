namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for element aliases (part of HashSet&lt;string&gt;).
/// </summary>
public sealed class AliasParameterType : IParameterTypeHandler
{
    public string Key => "alias";
    
    public string DisplayName => "Alias";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_String";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
