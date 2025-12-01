namespace AdventureGame.Engine.Parameters;

/// <summary>
/// Defines a discoverable parameter type that can be used by both Conditions and Effects.
/// Parameter types define how values are edited (via EditorComponent) and deserialized.
/// </summary>
public interface IParameterTypeHandler
{
    /// <summary>
    /// Unique identifier for this parameter type (e.g., "gameElement", "number", "boolean")
    /// </summary>
    string Key { get; }
    
    /// <summary>
    /// Human-readable name shown in the editor UI
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The fully qualified type name of the Blazor editor component used to edit this parameter type.
    /// Must inherit from ParameterEditorBase.
    /// Example: "AdventureGame.Components.Parameters.Editors.ParamEditor_GameElement"
    /// </summary>
    string EditorComponentTypeName { get; }

    /// <summary>
    /// Convert raw JSON-loaded values to the strongly typed value.
    /// This is called during deserialization to transform stored parameter values.
    /// </summary>
    /// <param name="rawValue">The raw value from JSON or string storage</param>
    /// <returns>The typed value to be used by the condition/effect</returns>
    object? Deserialize(object? rawValue);
}
