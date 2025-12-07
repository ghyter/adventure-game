namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for element parent ID.
/// </summary>
public sealed class ParentIdParameterType : IParameterTypeHandler
{
    public string Key => "parentId";
    
    public string DisplayName => "Parent ID";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_GameElement";
    
    public object? Deserialize(object? rawValue)
    {
        if (rawValue is null) return null;
        return rawValue.ToString();
    }
}
