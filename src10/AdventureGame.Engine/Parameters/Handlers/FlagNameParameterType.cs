namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for flag names in game element dictionaries.
/// </summary>
public sealed class FlagNameParameterType : IParameterTypeHandler
{
    public string Key => "flagName";
    
    public string DisplayName => "Flag Name";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_FlagName";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString();
    }
}
