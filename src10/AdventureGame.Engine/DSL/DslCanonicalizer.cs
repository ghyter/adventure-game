namespace AdventureGame.Engine.DSL;

using System.Text.RegularExpressions;

/// <summary>
/// Interface for DSL canonicalization.
/// Converts natural language DSL into canonical form suitable for parsing.
/// </summary>
public interface IDslCanonicalizer
{
    /// <summary>
    /// Canonicalizes input DSL text using the provided vocabulary.
    /// </summary>
    string Canonicalize(string input, DslVocabulary vocab);
}

/// <summary>
/// Default implementation of IDslCanonicalizer.
/// Handles:
/// 1. Possessive and "of" constructions
/// 2. Determiner removal
/// 3. Multi-word operator normalization
/// 4. Multi-word identifier normalization
/// </summary>
public sealed partial class DslCanonicalizer : IDslCanonicalizer
{
    // Known subjects in the DSL
    private static readonly HashSet<string> KnownSubjects = new(StringComparer.OrdinalIgnoreCase)
    {
        "player", "target", "target2", "scene", "item", "npc", "exit", "currentscene"
    };

    // Multi-word operator phrases to canonical forms
    private static readonly Dictionary<string, string> OperatorPhrases = new(StringComparer.OrdinalIgnoreCase)
    {
        { "is less than", "is_less_than" },
        { "is greater than", "is_greater_than" },
        { "is equal to", "is_equal_to" },
        { "is not equal to", "is_not_equal_to" },
        { "is in", "is_in" },
        { "distance from", "distance_from" }
    };

    // Determiners to remove
    private static readonly HashSet<string> Determiners = new(StringComparer.OrdinalIgnoreCase)
    {
        // Do not remove single-letter identifier "a" since it can be a valid identifier in tests
        "the", "an"
    };

    /// <summary>
    /// Canonicalizes the input DSL text.
    /// </summary>
    public string Canonicalize(string input, DslVocabulary vocab)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var text = input.Trim();

        // Step 1: Handle possessive and "of" constructions
        text = HandlePossessives(text);

        // Step 2: Remove determiners
        text = RemoveDeterminers(text);

        // Step 3: Replace multi-word operators (case-insensitive, whole phrase)
        text = ReplaceOperatorPhrases(text);

        // Step 4: Replace multi-word identifiers using vocabulary
        text = ReplaceIdentifiers(text, vocab);

        return text;
    }

    /// <summary>
    /// Handles possessive constructions and "of" phrases.
    /// Examples:
    /// - "player's state" -> "player.state"
    /// - "state of the player" -> "player.state"
    /// </summary>
    private static string HandlePossessives(string text)
    {
        // Replace possessive 's with dot
        text = PossessiveRegex().Replace(text, "$1.");

        // Handle "X of Y" patterns where Y is a known subject
        foreach (var subject in KnownSubjects)
        {
            // "X of the subject" -> "subject.X"
            var pattern = $@"\b([a-z_]+)\s+of\s+(the\s+)?{Regex.Escape(subject)}\b";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var attribute = match.Groups[1].Value;
                text = Regex.Replace(text, pattern, $"{subject}.{attribute}", RegexOptions.IgnoreCase);
            }
        }

        return text;
    }

    /// <summary>
    /// Removes leading determiners from tokens.
    /// </summary>
    private static string RemoveDeterminers(string text)
    {
        // Split by whitespace
        var tokens = text.Split([' ', '\t'], System.StringSplitOptions.RemoveEmptyEntries);
        var result = new System.Collections.Generic.List<string>();

        foreach (var token in tokens)
        {
            if (!Determiners.Contains(token))
            {
                result.Add(token);
            }
        }

        return string.Join(" ", result);
    }

    /// <summary>
    /// Replaces multi-word operator phrases with canonical single-word versions.
    /// </summary>
    private static string ReplaceOperatorPhrases(string text)
    {
        var result = text;

        // Sort by descending word count to match longest phrases first
        var sortedPhrases = OperatorPhrases
            .OrderByDescending(kvp => kvp.Key.Split(' ').Length)
            .ToList();

        foreach (var (phrase, canonical) in sortedPhrases)
        {
            // Match whole phrase boundaries
            var pattern = $@"\b{Regex.Escape(phrase)}\b";
            result = Regex.Replace(result, pattern, canonical, RegexOptions.IgnoreCase);
        }

        return result;
    }

    /// <summary>
    /// Replaces multi-word identifiers with their canonical forms from vocabulary.
    /// </summary>
    private static string ReplaceIdentifiers(string text, DslVocabulary vocab)
    {
        var result = text;

        if (vocab?.ElementNames == null) return result;

        // Sort by descending word count to match longest phrases first
        var sortedKeys = vocab.ElementNames.Keys
            .OrderByDescending(k => k.Split(' ').Length)
            .ToList();

        foreach (var phrase in sortedKeys)
        {
            if (vocab.ElementNames.TryGetValue(phrase, out var canonicalIds) && canonicalIds.Count > 0)
            {
                var canonical = canonicalIds[0]; // Use first mapping
                // Match whole word boundaries
                var pattern = $@"\b{Regex.Escape(phrase)}\b";
                result = Regex.Replace(result, pattern, canonical, RegexOptions.IgnoreCase);
            }
        }

        return result;
    }

    [GeneratedRegex(@"(\w+)'s\s+", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex PossessiveRegex();
}
