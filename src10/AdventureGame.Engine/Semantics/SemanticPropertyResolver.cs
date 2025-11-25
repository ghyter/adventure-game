namespace AdventureGame.Engine.Semantics;

using AdventureGame.Engine.Models;

/// <summary>
/// Defines the type of semantic property binding.
/// Resolution order: State > Flag > Property > Attribute
/// </summary>
public enum SemanticPropertyKind
{
    State,
    Flag,
    Property,
    Attribute
}

/// <summary>
/// Represents a resolved semantic property binding to an actual element field.
/// </summary>
public sealed class SemanticPropertyBinding
{
    public SemanticPropertyKind Kind { get; init; }
    public string Key { get; init; } = "";
    
    public SemanticPropertyBinding(SemanticPropertyKind kind, string key)
    {
        Kind = kind;
        Key = key;
    }
}

/// <summary>
/// Resolves natural language semantic names (e.g., "open", "closed", "lit")
/// to actual element data fields using a precedence system.
/// </summary>
public class SemanticPropertyResolver
{
    /// <summary>
    /// Resolves a semantic name to the most appropriate element field.
    /// Resolution precedence (highest to lowest):
    /// 1. State names
    /// 2. Flags
    /// 3. Properties
    /// 4. Attributes
    /// </summary>
    /// <param name="element">The game element to search</param>
    /// <param name="semanticName">The semantic name to resolve (e.g., "open", "lit")</param>
    /// <returns>A binding if found, otherwise null</returns>
    public SemanticPropertyBinding? Resolve(GameElement element, string semanticName)
    {
        if (element == null || string.IsNullOrWhiteSpace(semanticName))
            return null;

        // Normalize for case-insensitive comparison
        var normalized = semanticName.Trim();

        // 1. Check States (highest priority)
        if (element.States.ContainsKey(normalized))
        {
            return new SemanticPropertyBinding(SemanticPropertyKind.State, normalized);
        }

        // 2. Check Flags
        if (element.Flags.ContainsKey(normalized))
        {
            return new SemanticPropertyBinding(SemanticPropertyKind.Flag, normalized);
        }

        // 3. Check Properties
        if (element.Properties.ContainsKey(normalized))
        {
            return new SemanticPropertyBinding(SemanticPropertyKind.Property, normalized);
        }

        // 4. Check Attributes (lowest priority)
        if (element.Attributes.ContainsKey(normalized))
        {
            return new SemanticPropertyBinding(SemanticPropertyKind.Attribute, normalized);
        }

        // No match found
        return null;
    }

    /// <summary>
    /// Gets the current value of a semantic property from an element.
    /// </summary>
    public object? GetValue(GameElement element, SemanticPropertyBinding binding)
    {
        if (element == null || binding == null)
            return null;

        return binding.Kind switch
        {
            SemanticPropertyKind.State => element.Properties.TryGetValue("CurrentState", out var state) && !string.IsNullOrWhiteSpace(state)
                ? state
                : element.DefaultState,
            SemanticPropertyKind.Flag => element.Flags.TryGetValue(binding.Key, out var flag) ? flag : null,
            SemanticPropertyKind.Property => element.Properties.TryGetValue(binding.Key, out var prop) ? prop : null,
            SemanticPropertyKind.Attribute => element.Attributes.TryGetValue(binding.Key, out var attr) ? attr : null,
            _ => null
        };
    }

    /// <summary>
    /// Sets the value of a semantic property on an element.
    /// </summary>
    public void SetValue(GameElement element, SemanticPropertyBinding binding, object? value)
    {
        if (element == null || binding == null)
            return;

        switch (binding.Kind)
        {
            case SemanticPropertyKind.State:
                // Set CurrentState property
                element.Properties["CurrentState"] = value?.ToString() ?? binding.Key;
                break;

            case SemanticPropertyKind.Flag:
                if (value is bool boolValue)
                {
                    element.Flags[binding.Key] = boolValue;
                }
                else if (bool.TryParse(value?.ToString(), out var parsedBool))
                {
                    element.Flags[binding.Key] = parsedBool;
                }
                break;

            case SemanticPropertyKind.Property:
                element.Properties[binding.Key] = value?.ToString() ?? "";
                break;

            case SemanticPropertyKind.Attribute:
                if (value is int intValue)
                {
                    element.Attributes[binding.Key] = intValue;
                }
                else if (int.TryParse(value?.ToString(), out var parsedInt))
                {
                    element.Attributes[binding.Key] = parsedInt;
                }
                break;
        }
    }

    /// <summary>
    /// Increments a numeric semantic property.
    /// </summary>
    public bool Increment(GameElement element, SemanticPropertyBinding binding, int amount)
    {
        if (element == null || binding == null)
            return false;

        var currentValue = GetValue(element, binding);

        // Only attributes support increment (they're integers)
        if (binding.Kind == SemanticPropertyKind.Attribute && currentValue is int intValue)
        {
            element.Attributes[binding.Key] = intValue + amount;
            return true;
        }

        // For properties that hold numeric strings
        if (binding.Kind == SemanticPropertyKind.Property && int.TryParse(currentValue?.ToString(), out var propInt))
        {
            element.Properties[binding.Key] = (propInt + amount).ToString();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Decrements a numeric semantic property.
    /// </summary>
    public bool Decrement(GameElement element, SemanticPropertyBinding binding, int amount)
    {
        return Increment(element, binding, -amount);
    }
}
