namespace AdventureGame.Engine.Parameters.Handlers;

/// <summary>
/// Parameter type handler for comparison operators.
/// </summary>
public sealed class ComparisonOperatorParameterType : IParameterTypeHandler
{
    public string Key => "comparisonOperator";
    
    public string DisplayName => "Comparison Operator";
    
    public string EditorComponentTypeName => "AdventureGame.Components.Parameters.Editors.ParamEditor_ComparisonOperator";
    
    public object? Deserialize(object? rawValue)
    {
        return rawValue?.ToString()?.ToLowerInvariant();
    }
}
