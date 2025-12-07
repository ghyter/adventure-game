namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for element kind/type (item, npc, scene, exit, player, level).
/// </summary>
public sealed class ElementKindParameterType : IParameterTypeHandler
{
    public string Key => "elementKind";
    
    public string DisplayName => "Element Kind";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_ElementKind";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString()?.ToLowerInvariant();
    }
}
