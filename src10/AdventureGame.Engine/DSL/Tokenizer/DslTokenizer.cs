namespace AdventureGame.Engine.DSL.Tokenizer;

using System.Globalization;

/// <summary>
/// Tokenizes DSL text into a list of tokens.
/// </summary>
public class DslTokenizer(string input)
{
    private readonly string _input = input ?? string.Empty;
    private int _position = 0;
    private readonly List<Token> _tokens = [];

    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "and", TokenType.And },
        { "or", TokenType.Or },
        { "not", TokenType.Not },
        { "is", TokenType.Is },
        { "is_not", TokenType.IsNot },
        { "is_less_than", TokenType.IsLessThan },
        { "is_greater_than", TokenType.IsGreaterThan },
        { "is_equal_to", TokenType.IsEqualTo },
        { "is_not_equal_to", TokenType.IsNotEqualTo },
        { "is_in", TokenType.IsIn },
        { "is_empty", TokenType.IsEmpty },
        { "has", TokenType.IsIn }, // "has" is an alias for "is_in"
        { "attribute", TokenType.Attribute },
        { "state", TokenType.State },
        { "flag", TokenType.Flag },
        { "inventory", TokenType.Inventory },
        { "visits", TokenType.Visits },
        { "distance_from", TokenType.DistanceFrom },
        { "true", TokenType.True },
        { "false", TokenType.False },
    };

    public List<Token> Tokenize()
    {
        while (_position < _input.Length)
        {
            char current = _input[_position];

            // Skip whitespace
            if (char.IsWhiteSpace(current))
            {
                _position++;
                continue;
            }

            // String literals
            if (current == '"')
            {
                TokenizeStringLiteral();
                continue;
            }

            // Parentheses
            if (current == '(')
            {
                _tokens.Add(new Token(TokenType.ParenOpen, "(", _position, _position));
                _position++;
                continue;
            }

            if (current == ')')
            {
                _tokens.Add(new Token(TokenType.ParenClose, ")", _position, _position));
                _position++;
                continue;
            }

            // Dot
            if (current == '.')
            {
                _tokens.Add(new Token(TokenType.Dot, ".", _position, _position));
                _position++;
                continue;
            }

            // Apostrophe for possessive
            if (current == '\'')
            {
                if (_position + 1 < _input.Length && _input[_position + 1] == 's')
                {
                    _tokens.Add(new Token(TokenType.Identifier, "'s", _position, _position + 1));
                    _position += 2;
                    continue;
                }
            }

            // Numbers (including decimals)
            if (char.IsDigit(current) || (current == '-' && _position + 1 < _input.Length && char.IsDigit(_input[_position + 1])))
            {
                TokenizeNumber();
                continue;
            }

            // Identifiers and keywords (including underscores)
            if (char.IsLetter(current) || current == '_')
            {
                TokenizeIdentifierOrKeyword();
                continue;
            }

            // Unknown character
            _tokens.Add(new Token(TokenType.Unknown, current.ToString(), _position, _position));
            _position++;
        }

        _tokens.Add(new Token(TokenType.EndOfInput, "", _position, _position));
        return _tokens;
    }

    private void TokenizeStringLiteral()
    {
        int start = _position;
        _position++; // Skip opening quote

        while (_position < _input.Length && _input[_position] != '"')
        {
            // Handle escaped quotes
            if (_input[_position] == '\\' && _position + 1 < _input.Length && _input[_position + 1] == '"')
            {
                _position += 2;
            }
            else
            {
                _position++;
            }
        }

        if (_position < _input.Length)
        {
            _position++; // Skip closing quote
        }

        string value = _input.Substring(start + 1, _position - start - 2); // Extract content without quotes
        _tokens.Add(new Token(TokenType.StringLiteral, value, start, _position - 1));
    }

    private void TokenizeNumber()
    {
        int start = _position;
        
        // Handle negative sign
        if (_input[_position] == '-')
        {
            _position++;
        }

        // Read digits
        while (_position < _input.Length && char.IsDigit(_input[_position]))
        {
            _position++;
        }

        // Check for decimal point
        if (_position < _input.Length && _input[_position] == '.')
        {
            _position++;
            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }
        }

        string value = _input[start.._position];
        _tokens.Add(new Token(TokenType.Number, value, start, _position - 1));
    }

    private void TokenizeIdentifierOrKeyword()
    {
        int start = _position;

        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
        {
            _position++;
        }

        string value = _input[start.._position];

        // Check if it's a keyword
        TokenType type = Keywords.TryGetValue(value, out var keywordType) ? keywordType : TokenType.Identifier;

        _tokens.Add(new Token(type, value, start, _position - 1));
    }
}
