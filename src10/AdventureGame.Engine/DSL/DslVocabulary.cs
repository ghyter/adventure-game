namespace AdventureGame.Engine.DSL;

using AdventureGame.Engine.Models;
using System.Collections.Generic;

/// <summary>
/// Holds the vocabulary of a GamePack, including canonical forms for multi-word identifiers,
/// attribute names, flag names, and phrase-to-canonical mappings.
/// </summary>
public sealed class DslVocabulary
{
    /// <summary>
    /// Maps element names and aliases to canonical identifiers.
    /// Key: lowercase name/alias, Value: list of canonical IDs
    /// </summary>
    public Dictionary<string, List<string>> ElementNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Set of valid attribute names.
    /// </summary>
    public HashSet<string> AttributeNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Set of valid flag names.
    /// </summary>
    public HashSet<string> FlagNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Maps natural language phrases to canonical forms.
    /// E.g., "jade key" -> "jade_key", "is less than" -> "is_less_than"
    /// </summary>
    public Dictionary<string, string> PhraseToCanonical { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Builds a DslVocabulary from a GamePack.
    /// </summary>
    public static DslVocabulary FromGamePack(GamePack pack)
    {
        var vocab = new DslVocabulary();

        if (pack?.Elements == null) return vocab;

        // Register element names and aliases
        foreach (var element in pack.Elements)
        {
            if (string.IsNullOrWhiteSpace(element.Name)) continue;

            var canonicalId = element.Name.ToLowerInvariant().Replace(" ", "_");
            
            // Register the element's name
            if (!vocab.ElementNames.TryGetValue(element.Name, out List<string>? value))
            {
                value = [];
                vocab.ElementNames[element.Name] = value;
            }

            value.Add(canonicalId);

            // Register aliases
            foreach (var alias in element.Aliases)
            {
                if (!vocab.ElementNames.TryGetValue(alias, out List<string>? value1))
                {
                    value1 = [];
                    vocab.ElementNames[alias] = value1;
                }

                value1.Add(canonicalId);
            }

            // Register attribute keys
            foreach (var attrName in element.Attributes.Keys)
            {
                vocab.AttributeNames.Add(attrName);
            }

            // Register flag keys
            foreach (var flagName in element.Flags.Keys)
            {
                vocab.FlagNames.Add(flagName);
            }
        }

        return vocab;
    }
}
