namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for output mode selection.
/// </summary>
public sealed class OutputModeParameterType : IParameterTypeHandler
{
    public string Key => "outputMode";
    
    public string DisplayName => "Output Mode";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_OutputMode";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString()?.ToLowerInvariant() ?? "both";
    }
}
