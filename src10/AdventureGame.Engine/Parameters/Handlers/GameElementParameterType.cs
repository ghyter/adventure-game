namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for game element references.
/// Allows selection of any game element by ID.
/// </summary>
public sealed class GameElementParameterType : IParameterTypeHandler
{
    public string Key => "gameElement";
    
    public string DisplayName => "Game Element";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_GameElement";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
