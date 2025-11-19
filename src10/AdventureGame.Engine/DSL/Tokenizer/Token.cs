namespace AdventureGame.Engine.DSL.Tokenizer;

/// <summary>
/// Token type enumeration for DSL lexing.
/// </summary>
public enum TokenType
{
    // Literals
    Identifier,
    Number,
    StringLiteral,

    // Keywords
    And,
    Or,
    Not,
    Is,
    IsNot,
    IsLessThan,
    IsGreaterThan,
    IsEqualTo,
    IsNotEqualTo,
    IsIn,
    IsEmpty,
    Attribute,
    State,
    Flag,
    Inventory,
    Visits,
    DistanceFrom,
    True,
    False,

    // Syntax
    ParenOpen,
    ParenClose,
    Dot,

    // Special
    EndOfInput,
    Unknown
}

/// <summary>
/// Represents a single token in the DSL input.
/// </summary>
public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }

    public Token(TokenType type, string value, int startIndex, int endIndex)
    {
        Type = type;
        Value = value;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public override string ToString() => $"{Type}: '{Value}' [{StartIndex}..{EndIndex}]";
}
