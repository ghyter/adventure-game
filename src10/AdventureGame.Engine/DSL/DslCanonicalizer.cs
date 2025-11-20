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
/// 5. Natural language condition prefixes (when, if)
/// 6. Implicit subject inference for known elements
/// 7. Effect keywords (set, make, update, increment)
/// </summary>
public class DslCanonicalizer : IDslCanonicalizer
{
    // Known subjects in the DSL
    private static readonly HashSet<string> KnownSubjects = new(StringComparer.OrdinalIgnoreCase)
    {
        "player", "target", "target2", "scene", "item", "npc", "exit", "currentscene", "session", "log"
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

    // Condition prefix words to strip (they're implied)
    private static readonly HashSet<string> ConditionPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "when", "if", "while"
    };

    // Effect prefix words to strip (they indicate mutation operations)
    private static readonly HashSet<string> EffectPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "set", "make", "update", "change", "increment", "decrement", "add", "remove"
    };

    /// <summary>
    /// Canonicalizes the input DSL text.
    /// </summary>
    public string Canonicalize(string input, DslVocabulary vocab)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var text = input.Trim();

        // Step 1: Strip condition/effect prefix words
        text = StripPrefixWords(text);

        // Step 2: Handle possessive and "of" constructions
        text = HandlePossessives(text);

        // Step 3: Infer implicit subjects for known elements
        text = InferImplicitSubjects(text, vocab);

        // Step 4: Remove determiners
        text = RemoveDeterminers(text);

        // Step 5: Replace multi-word operators (case-insensitive, whole phrase)
        text = ReplaceOperatorPhrases(text);

        // Step 6: Replace multi-word identifiers using vocabulary
        text = ReplaceIdentifiers(text, vocab);

        return text;
    }

    /// <summary>
    /// Strips condition prefix words like "when", "if" and effect prefix words like "set", "make".
    /// Examples:
    /// - "when desk state is closed" -> "desk state is closed"
    /// - "set the property x to hello" -> "the property x to hello"
    /// </summary>
    private string StripPrefixWords(string text)
    {
        var tokens = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return text;

        // Check if first token is a prefix word
        if (ConditionPrefixes.Contains(tokens[0]) || EffectPrefixes.Contains(tokens[0]))
        {
            // Remove the first token
            return string.Join(" ", tokens.Skip(1));
        }

        return text;
    }

    /// <summary>
    /// Infers implicit subjects for known game elements.
    /// Examples:
    /// - "desk state is closed" -> "item desk.state is closed" (if "desk" is in vocabulary)
    /// - "guard is visible" -> "npc guard is visible" (if "guard" is an NPC)
    /// </summary>
    private string InferImplicitSubjects(string text, DslVocabulary vocab)
    {
        if (vocab?.ElementNames == null) return text;

        var tokens = text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0) return text;

        var firstToken = tokens[0];

        // If first token is already a known subject, don't infer
        if (KnownSubjects.Contains(firstToken))
            return text;

        // Check if first token (or multi-word phrase) is a known element
        // Try progressively longer phrases starting from the beginning
        for (int wordCount = Math.Min(3, tokens.Length); wordCount >= 1; wordCount--)
        {
            var phrase = string.Join(" ", tokens.Take(wordCount));
            
            if (vocab.ElementNames.TryGetValue(phrase, out var canonicalIds) && canonicalIds.Count > 0)
            {
                // Found a known element - need to determine its type and prepend it
                // Look it up in the vocabulary to determine type
                var elementType = DetermineElementType(phrase, vocab);
                
                if (!string.IsNullOrEmpty(elementType))
                {
                    // Prepend "type elementName" and keep the rest
                    var remainder = string.Join(" ", tokens.Skip(wordCount));
                    return $"{elementType} {phrase} {remainder}".Trim();
                }
            }
        }

        return text;
    }

    /// <summary>
    /// Determines the element type (item, npc, scene, etc.) for a given element name.
    /// This is a heuristic - in a real implementation, you'd query the GamePack.
    /// </summary>
    private string DetermineElementType(string elementName, DslVocabulary vocab)
    {
        // This is a simplified heuristic
        // In a full implementation, you would need access to the GamePack to check element types
        // For now, default to "item" as it's the most common case
        
        // Common NPC names/patterns
        if (Regex.IsMatch(elementName, @"\b(guard|knight|wizard|merchant|king|queen|prince|princess|monster|dragon|goblin)\b", RegexOptions.IgnoreCase))
            return "npc";
        
        // Common scene/location names
        if (Regex.IsMatch(elementName, @"\b(room|hall|chamber|kitchen|bedroom|dungeon|tower|castle|forest|cave)\b", RegexOptions.IgnoreCase))
            return "scene";
        
        // Default to item for most physical objects
        return "item";
    }

    /// <summary>
    /// Handles possessive constructions and "of" phrases.
    /// Examples:
    /// - "player's state" -> "player.state"
    /// - "state of the player" -> "player.state"
    /// - "desk's state" -> "desk.state"
    /// </summary>
    private string HandlePossessives(string text)
    {
        // Replace possessive 's with dot
        text = Regex.Replace(text, @"(\w+)'s\s+", "$1.", RegexOptions.IgnoreCase);

        // Handle "X of Y" patterns where Y is a known subject or element name
        // This is tricky because we don't have the vocabulary here yet
        // So we'll handle it for known subjects
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
    private string RemoveDeterminers(string text)
    {
        // Split by whitespace
        var tokens = text.Split(new[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
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
    private string ReplaceOperatorPhrases(string text)
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
    private string ReplaceIdentifiers(string text, DslVocabulary vocab)
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
}
