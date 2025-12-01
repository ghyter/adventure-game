using AdventureGame.Engine.Parameters;
using Microsoft.AspNetCore.Components;

namespace AdventureGame.Components.Parameters;

/// <summary>
/// Base class for all parameter editor components.
/// Provides access to the parameter descriptor, current values, and value change notifications.
/// </summary>
public abstract class ParameterEditorBase : ComponentBase
{
    /// <summary>
    /// The descriptor for this parameter (name, type, etc.)
    /// </summary>
    [Parameter] 
    public ParameterDescriptor Descriptor { get; set; } = default!;
    
    /// <summary>
    /// Dictionary containing all parameter values (shared across all parameters)
    /// </summary>
    [Parameter] 
    public Dictionary<string, object?> Values { get; set; } = default!;
    
    /// <summary>
    /// Service provider for accessing game services (GameSession, etc.)
    /// </summary>
    [Parameter] 
    public IServiceProvider Services { get; set; } = default!;
    
    /// <summary>
    /// Event callback invoked when this parameter's value changes
    /// </summary>
    [Parameter] 
    public EventCallback<object?> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the current value for this parameter
    /// </summary>
    protected object? CurrentValue
    {
        get => Values.TryGetValue(Descriptor.Name, out var val) ? val : null;
        set
        {
            Values[Descriptor.Name] = value;
            ValueChanged.InvokeAsync(value);
        }
    }
    
    /// <summary>
    /// Gets the current value as a string (for text-based editors)
    /// </summary>
    protected string? CurrentValueAsString
    {
        get => CurrentValue?.ToString();
        set => CurrentValue = value;
    }
    
    /// <summary>
    /// Gets the current value as an int (for numeric editors)
    /// </summary>
    protected int CurrentValueAsInt
    {
        get => CurrentValue is int i ? i : (int.TryParse(CurrentValue?.ToString(), out var parsed) ? parsed : 0);
        set => CurrentValue = value;
    }
    
    /// <summary>
    /// Gets the current value as a bool (for boolean editors)
    /// </summary>
    protected bool CurrentValueAsBool
    {
        get => CurrentValue is bool b ? b : (bool.TryParse(CurrentValue?.ToString(), out var parsed) && parsed);
        set => CurrentValue = value;
    }
}
