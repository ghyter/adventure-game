namespace AdventureGame.Engine.DSL;

using AdventureGame.Engine.DSL.Tokenizer;
using AdventureGame.Engine.DSL.AST;
using AdventureGame.Engine.DSL.AST.Effects;
using System.Globalization;

/// <summary>
/// Parser for Effects DSL.
/// Parses effect statements into EffectNode AST objects.
/// </summary>
public class EffectDslParser
{
    private List<Token> _tokens = [];
    private int _position = 0;

    /// <summary>
    /// Parses a single effect statement.
    /// </summary>
    public EffectNode? Parse(string effectText)
    {
        if (string.IsNullOrWhiteSpace(effectText))
            return null;

        // Tokenize
        var tokenizer = new DslTokenizer(effectText);
        _tokens = tokenizer.Tokenize();
        _position = 0;

        // Parse the effect
        return ParseEffect();
    }

    /// <summary>
    /// Parses multiple effect statements.
    /// </summary>
    public List<EffectNode> ParseMultiple(string effectsText)
    {
        var effects = new List<EffectNode>();

        if (string.IsNullOrWhiteSpace(effectsText))
            return effects;

        // Split by periods or semicolons
        var statements = effectsText.Split(['.', ';'], System.StringSplitOptions.RemoveEmptyEntries);

        foreach (var statement in statements)
        {
            var effect = Parse(statement.Trim());
            if (effect != null)
            {
                effects.Add(effect);
            }
        }

        return effects;
    }

    private EffectNode? ParseEffect()
    {
        if (_position >= _tokens.Count) return null;

        var token = _tokens[_position];

        // Match effect patterns
        if (IsKeyword("set"))
        {
            return ParseSetEffect();
        }
        else if (IsKeyword("move"))
        {
            return ParseMoveEffect();
        }
        else if (IsKeyword("give") || IsKeyword("add") || IsKeyword("remove"))
        {
            return ParseInventoryEffect();
        }
        else if (IsKeyword("increase") || IsKeyword("decrease"))
        {
            return ParseAttributeEffect();
        }
        else if (IsKeyword("say"))
        {
            return ParseSayEffect();
        }

        return null;
    }

    private EffectNode? ParseSetEffect()
    {
        // Pattern: set <subject>'s state to <state>
        // Pattern: set <subject>'s flag <name> to true|false
        // Pattern: set <subject>'s attribute <name> to <value>

        Expect("set");

        var subject = ParseSubject();
        if (subject == null) return null;

        Expect("'s");

        var nextKeyword = PeekKeyword();

        if (nextKeyword == "state")
        {
            Advance(); // consume "state"
            Expect("to");
            var stateName = ExpectIdentifier();
            return new SetStateEffect { Subject = subject, StateName = stateName };
        }
        else if (nextKeyword == "flag")
        {
            Advance(); // consume "flag"
            var flagName = ExpectIdentifier();
            Expect("to");
            var value = ExpectBool();
            return new FlagEffect { Subject = subject, FlagName = flagName, Value = value };
        }
        else if (nextKeyword == "attribute")
        {
            Advance(); // consume "attribute"
            var attrName = ExpectIdentifier();
            Expect("to");
            var value = ExpectNumber();
            return new AttributeEffect { Subject = subject, AttributeName = attrName, Operation = "set", Value = value };
        }

        return null;
    }

    private EffectNode? ParseMoveEffect()
    {
        // Pattern: move <subject> to <target>
        Expect("move");

        var subject = ParseSubject();
        if (subject == null) return null;

        Expect("to");

        var target = ParseSubject();
        if (target == null) return null;

        return new MoveEffect { Subject = subject, Target = target };
    }

    private EffectNode? ParseInventoryEffect()
    {
        // Pattern: give <item> to <subject>
        // Pattern: add <item> to <subject>
        // Pattern: remove <item> from <subject>

        var operation = ExpectKeyword(["give", "add", "remove"]);
        if (operation == null) return null;

        var item = ParseSubject();
        if (item == null) return null;

        if (operation == "remove")
        {
            Expect("from");
        }
        else
        {
            Expect("to");
        }

        var subject = ParseSubject();
        if (subject == null) return null;

        return new InventoryEffect { Operation = operation, Item = item, Subject = subject };
    }

    private EffectNode? ParseAttributeEffect()
    {
        // Pattern: increase <subject>'s attribute <name> by <value>
        // Pattern: decrease <subject>'s attribute <name> by <value>

        var operation = ExpectKeyword(["increase", "decrease"]);
        if (operation == null) return null;

        var subject = ParseSubject();
        if (subject == null) return null;

        Expect("'s");
        Expect("attribute");

        var attrName = ExpectIdentifier();
        Expect("by");

        var value = ExpectNumber();

        return new AttributeEffect { Subject = subject, AttributeName = attrName, Operation = operation, Value = value };
    }

    private EffectNode? ParseSayEffect()
    {
        // Pattern: say "message"
        Expect("say");

        var message = ExpectString();
        if (message == null) return null;

        return new SayEffect { Message = message };
    }

    private SubjectRef? ParseSubject()
    {
        if (_position >= _tokens.Count) return null;

        var token = _tokens[_position];

        // Check for known subjects
        if (token.Type == TokenType.Identifier)
        {
            var keyword = token.Value.ToLowerInvariant();
            
            if (keyword is "player" or "target" or "target2" or "scene" or "item" or "npc" or "exit" or "currentscene")
            {
                _position++;
                
                // Only consume an ID for subjects that require one: item, npc, scene, exit
                if (keyword is "item" or "npc" or "scene" or "exit")
                {
                    // Check for ID reference (e.g., "item sword" or "npc guard")
                    if (_position < _tokens.Count && _tokens[_position].Type == TokenType.Identifier)
                    {
                        var id = _tokens[_position].Value;
                        _position++;
                        return new SubjectRef { Kind = keyword, Id = id };
                    }
                }
                
                return new SubjectRef { Kind = keyword };
            }
        }

        return null;
    }

    private bool IsKeyword(string keyword) => PeekKeyword() == keyword;

    private string? PeekKeyword()
    {
        if (_position >= _tokens.Count) return null;
        
        var token = _tokens[_position];
        
        // Accept any token that has a string value (Identifier or any keyword token)
        return token.Value?.ToLowerInvariant();
    }

    private void Expect(string keyword)
    {
        if (_position >= _tokens.Count)
            throw new InvalidOperationException($"Expected '{keyword}' but reached end of input");

        var token = _tokens[_position];

        // Special handling for 's - it's optional and tokenized as an identifier
        if (keyword == "'s")
        {
            // If we find the 's token, consume it
            if (token.Type == TokenType.Identifier && token.Value == "'s")
            {
                _position++;
            }
            // If not present, that's OK - we can proceed without it
            return;
        }

        // For keywords that match special TokenTypes, check those too
        var expectedKeywordLower = keyword.ToLowerInvariant();
        var tokenValueLower = token.Value.ToLowerInvariant();
        
        // Accept either Identifier type OR any keyword token type if the value matches
        bool isMatch = tokenValueLower == expectedKeywordLower;
        
        if (!isMatch)
        {
            throw new InvalidOperationException($"Expected '{keyword}' but found '{token.Value}'");
        }

        _position++;
    }

    private string? ExpectKeyword(params string[] keywords)
    {
        if (_position >= _tokens.Count) return null;

        var token = _tokens[_position];
        
        // Accept any token type as long as the value matches one of the expected keywords
        var lower = token.Value?.ToLowerInvariant();
        if (lower == null || !keywords.Contains(lower)) return null;

        _position++;
        return lower;
    }

    private string ExpectIdentifier()
    {
        if (_position >= _tokens.Count)
            throw new InvalidOperationException("Expected identifier but reached end of input");

        var token = _tokens[_position];
        if (token.Type != TokenType.Identifier)
            throw new InvalidOperationException($"Expected identifier but found {token.Type}");

        var value = token.Value;
        _position++;
        return value;
    }

    private string? ExpectString()
    {
        if (_position >= _tokens.Count) return null;

        var token = _tokens[_position];
        if (token.Type != TokenType.StringLiteral) return null;

        var value = token.Value;
        _position++;
        return value;
    }

    private double ExpectNumber()
    {
        if (_position >= _tokens.Count)
            throw new InvalidOperationException("Expected number but reached end of input");

        var token = _tokens[_position];
        if (token.Type != TokenType.Number)
            throw new InvalidOperationException($"Expected number but found {token.Type}");

        if (!double.TryParse(token.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
            throw new InvalidOperationException($"Could not parse '{token.Value}' as number");

        _position++;
        return value;
    }

    private bool ExpectBool()
    {
        var keyword = ExpectKeyword("true", "false");
        return keyword == null ? throw new InvalidOperationException("Expected 'true' or 'false'") : keyword == "true";
    }

    private void Advance()
    {
        if (_position < _tokens.Count)
        {
            _position++;
        }
    }
}
