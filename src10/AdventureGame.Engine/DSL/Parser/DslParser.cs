namespace AdventureGame.Engine.DSL.Parser;

using AdventureGame.Engine.DSL.Tokenizer;
using AdventureGame.Engine.DSL.AST;
using System.Globalization;

/// <summary>
/// Recursive descent parser for DSL condition expressions.
/// </summary>
public class DslParser
{
    private List<Token> _tokens = [];
    private int _current = 0;
    private readonly DslParseResult _result = new() { Success = true };

    public DslParseResult Parse(string input)
    {
        _result.Success = true;
        _result.Errors.Clear();
        _result.Ast = null;

        // Tokenize
        var tokenizer = new DslTokenizer(input);
        _tokens = tokenizer.Tokenize();
        _current = 0;

        // Parse
        try
        {
            _result.Ast = Expression();

            // Ensure we consumed all tokens (except EndOfInput)
            if (!IsAtEnd())
            {
                Token token = Peek();
                AddError($"Unexpected token: {token.Value}", token.StartIndex, token.EndIndex);
            }
        }
        catch (Exception ex)
        {
            if (_result.Errors.Count == 0)
            {
                AddError($"Parse error: {ex.Message}", 0, _input.Length - 1);
            }
        }

        return _result;
    }

    // Grammar Rules
    // Expression -> OrExpr
    private ConditionNode Expression()
    {
        return OrExpr();
    }

    // OrExpr -> AndExpr ( "or" AndExpr )*
    private ConditionNode OrExpr()
    {
        ConditionNode expr = AndExpr();

        while (Match(TokenType.Or))
        {
            ConditionNode right = AndExpr();
            expr = new OrNode { Left = expr, Right = right };
        }

        return expr;
    }

    // AndExpr -> UnaryExpr ( "and" UnaryExpr )*
    private ConditionNode AndExpr()
    {
        ConditionNode expr = UnaryExpr();

        while (Match(TokenType.And))
        {
            ConditionNode right = UnaryExpr();
            expr = new AndNode { Left = expr, Right = right };
        }

        return expr;
    }

    // UnaryExpr -> "not" UnaryExpr | "(" Expression ")" | Relation
    private ConditionNode UnaryExpr()
    {
        if (Match(TokenType.Not))
        {
            ConditionNode expr = UnaryExpr();
            return new NotNode { Inner = expr };
        }

        if (Match(TokenType.ParenOpen))
        {
            ConditionNode expr = Expression();
            if (!Match(TokenType.ParenClose))
            {
                AddError("Expected ')'", Peek().StartIndex, Peek().EndIndex);
            }
            return expr;
        }

        return Relation();
    }

    // Relation -> PropertyAccess Comparison Value | CountRelation | DistanceRelation | HasRelation
    private ConditionNode Relation()
    {
        SubjectRef subject = ParseSubject();

        // Check for "has" keyword - inventory containment check
        if (PeekValue("has"))
        {
            Advance();  // Consume "has"
            SubjectRef targetSubject = ParseSubject();
            
            var hasRelation = new RelationNode
            {
                Subject = subject,
                Relation = "has",
                Object = new ObjectRef { Kind = "element", Value = targetSubject.Id ?? targetSubject.Kind }
            };
            return hasRelation;
        }

        // Check for distance_from - need to check both token type and value
        if (Peek().Type == TokenType.DistanceFrom || 
            (Peek().Type == TokenType.Dot && _current + 1 < _tokens.Count && 
             _tokens[_current + 1].Value.Equals("distance_from", StringComparison.OrdinalIgnoreCase)))
        {
            // Consume the dot if present
            if (Peek().Type == TokenType.Dot)
                Advance();
            return ParseDistanceRelation(subject);
        }

        // Check for visits - need to check both token type and value
        if (Peek().Type == TokenType.Visits || 
            (Peek().Type == TokenType.Dot && _current + 1 < _tokens.Count && 
             _tokens[_current + 1].Value.Equals("visits", StringComparison.OrdinalIgnoreCase)))
        {
            // Consume the dot if present
            if (Peek().Type == TokenType.Dot)
                Advance();
            return ParseCountRelation(subject);
        }

        // Standard property access relation
        return ParsePropertyRelation(subject);
    }

    private ConditionNode ParsePropertyRelation(SubjectRef subject)
    {
        // Parse property/attribute chain: .property.attribute, etc.
        string? propertyName = null;
        string? attributeName = null;

        // Check if there's an explicit property access (dot notation)
        if (Peek().Type == TokenType.Dot)
        {
            while (Match(TokenType.Dot))
            {
                // Accept any token type as long as it has a value (not just Identifier)
                var token = Peek();
                if (token.Type == TokenType.EndOfInput || string.IsNullOrEmpty(token.Value))
                {
                    AddError("Expected identifier after '.'", token.StartIndex, token.EndIndex);
                    throw new Exception("Parse error");
                }

                string fieldName = Advance().Value;

                // Check if this is "attribute <name>" or "state <name>" etc.
                if (fieldName.Equals("attribute", StringComparison.OrdinalIgnoreCase))
                {
                    var nextToken = Peek();
                    if (nextToken.Type == TokenType.EndOfInput || string.IsNullOrEmpty(nextToken.Value))
                    {
                        AddError("Expected attribute name after 'attribute'", nextToken.StartIndex, nextToken.EndIndex);
                        throw new Exception("Parse error");
                    }
                    attributeName = Advance().Value;
                }
                else if (fieldName.Equals("state", StringComparison.OrdinalIgnoreCase))
                {
                    // Next token is the property name
                    propertyName = "state";
                }
                else if (fieldName.Equals("flag", StringComparison.OrdinalIgnoreCase))
                {
                    propertyName = "flag";
                }
                else if (fieldName.Equals("inventory", StringComparison.OrdinalIgnoreCase))
                {
                    propertyName = "inventory";
                }
                else
                {
                    propertyName = fieldName;
                }
            }
        }
        else
        {
            // No explicit property access - peek ahead to see if we can infer one from context
            // For now, leave propertyName as null - the evaluator will handle intelligent fallback
        }

        // Parse comparison and value
        string comparison = ParseComparison();
        ObjectRef obj = ParseValue();

        var relation = new RelationNode
        {
            Subject = subject,
            Relation = comparison,
            Object = obj,
            AttributeName = attributeName,
            PropertyName = propertyName
        };

        return relation;
    }

    private ConditionNode ParseCountRelation(SubjectRef subject)
    {
        // visits scene_name comparison number
        Consume(TokenType.Visits, "Expected 'visits'");

        var token = Peek();
        if (token.Type == TokenType.EndOfInput || string.IsNullOrEmpty(token.Value))
        {
            AddError("Expected scene name after 'visits'", token.StartIndex, token.EndIndex);
            throw new Exception("Parse error");
        }

        string sceneName = Advance().Value;
        string comparison = ParseComparison();

        if (!Check(TokenType.Number))
        {
            AddError("Expected number in visits comparison", Peek().StartIndex, Peek().EndIndex);
            throw new Exception("Parse error");
        }

        int value = int.Parse(Advance().Value);

        return new CountRelationNode
        {
            Subject = subject,
            SceneName = sceneName,
            Comparison = comparison,
            Value = value
        };
    }

    private ConditionNode ParseDistanceRelation(SubjectRef subjectA)
    {
        // distance_from subject comparison number
        Consume(TokenType.DistanceFrom, "Expected 'distance_from'");

        SubjectRef subjectB = ParseSubject();
        string comparison = ParseComparison();

        if (!Check(TokenType.Number))
        {
            AddError("Expected number in distance comparison", Peek().StartIndex, Peek().EndIndex);
            throw new Exception("Parse error");
        }

        int value = int.Parse(Advance().Value);

        return new DistanceRelationNode
        {
            SubjectA = subjectA,
            SubjectB = subjectB,
            Comparison = comparison,
            Value = value
        };
    }

    private SubjectRef ParseSubject()
    {
        Token token = Advance();

        return token.Type switch
        {
            TokenType.Identifier when token.Value.Equals("player", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "player", Id = null },
            TokenType.Identifier when token.Value.Equals("target", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "target", Id = null },
            TokenType.Identifier when token.Value.Equals("target2", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "target2", Id = null },
            TokenType.Identifier when token.Value.Equals("currentScene", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "currentScene", Id = null },
            TokenType.Identifier when token.Value.Equals("location", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "location", Id = null },
            TokenType.Identifier when token.Value.Equals("session", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "session", Id = null },
            TokenType.Identifier when token.Value.Equals("log", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "log", Id = null },
            TokenType.Identifier when token.Value.Equals("item", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "item", Id = ParseElementId() },
            TokenType.Identifier when token.Value.Equals("npc", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "npc", Id = ParseElementId() },
            TokenType.Identifier when token.Value.Equals("scene", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "scene", Id = ParseElementId() },
            TokenType.Identifier when token.Value.Equals("exit", StringComparison.OrdinalIgnoreCase) =>
                new SubjectRef { Kind = "exit", Id = ParseElementId() },
            // Implicit element - no type prefix (e.g., "jade_key", "desk")
            // The canonicalizer should have added the type, but if not, default to "item"
            TokenType.Identifier =>
                new SubjectRef { Kind = "item", Id = token.Value }
        };
    }

    private string ParseElementId()
    {
        var token = Peek();
        if (token.Type == TokenType.EndOfInput || string.IsNullOrEmpty(token.Value))
        {
            AddError("Expected element identifier", token.StartIndex, token.EndIndex);
            throw new Exception("Parse error");
        }

        return Advance().Value;
    }

    private string ParseComparison()
    {
        Token token = Peek();

        if (token.Type == TokenType.IsLessThan || token.Type == TokenType.IsGreaterThan ||
            token.Type == TokenType.IsEqualTo || token.Type == TokenType.IsNotEqualTo ||
            token.Type == TokenType.Is || token.Type == TokenType.IsNot ||
            token.Type == TokenType.IsIn || token.Type == TokenType.IsEmpty)
        {
            return Advance().Value;
        }

        AddError($"Expected comparison operator, got '{token.Value}'", token.StartIndex, token.EndIndex);
        throw new Exception("Parse error");
    }

    private ObjectRef ParseValue()
    {
        Token token = Peek();

        if (token.Type == TokenType.True)
        {
            Advance();
            return new ObjectRef { Kind = "literal", Value = "true", BoolValue = true };
        }

        if (token.Type == TokenType.False)
        {
            Advance();
            return new ObjectRef { Kind = "literal", Value = "false", BoolValue = false };
        }

        if (token.Type == TokenType.Number)
        {
            Token numToken = Advance();
            if (double.TryParse(numToken.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double num))
            {
                return new ObjectRef { Kind = "literal", Value = numToken.Value, NumericValue = num };
            }
        }

        // Accept any token with a value as an identifier (including keyword tokens)
        if (!string.IsNullOrEmpty(token.Value) && token.Type != TokenType.EndOfInput)
        {
            string value = Advance().Value;
            return new ObjectRef { Kind = "element", Value = value };
        }

        AddError($"Expected value, got '{token.Value}'", token.StartIndex, token.EndIndex);
        throw new Exception("Parse error");
    }

    // Helper methods
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EndOfInput;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Previous()
    {
        return _tokens[_current - 1];
    }

    private void Consume(TokenType type, string message)
    {
        if (Check(type))
        {
            Advance();
            return;
        }
        
        // Also check by value for keywords that might be tokenized differently
        var token = Peek();
        if (type == TokenType.Visits && token.Value.Equals("visits", StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return;
        }
        
        if (type == TokenType.DistanceFrom && token.Value.Equals("distance_from", StringComparison.OrdinalIgnoreCase))
        {
            Advance();
            return;
        }

        AddError(message, token.StartIndex, token.EndIndex);
        throw new Exception("Parse error");
    }

    private bool PeekValue(string value)
    {
        return Peek().Value.Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    private void AddError(string message, int startIndex, int endIndex)
    {
        _result.AddError(message, startIndex, endIndex);
    }

    // Need _input for error reporting - store during Parse
    private string _input = string.Empty;

    public DslParseResult Parse(string input, int _unused = 0)
    {
        _input = input;
        return Parse(input);
    }
}
