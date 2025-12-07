namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for element tags (part of HashSet&lt;string&gt;).
/// </summary>
public sealed class TagParameterType : IParameterTypeHandler
{
    public string Key => "tag";
    
    public string DisplayName => "Tag";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_String";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
