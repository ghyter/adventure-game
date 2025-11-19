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
public class Token(TokenType type, string value, int startIndex, int endIndex)
{
    public TokenType Type { get; set; } = type;
    public string Value { get; set; } = value;
    public int StartIndex { get; set; } = startIndex;
    public int EndIndex { get; set; } = endIndex;

    public override string ToString() => $"{Type}: '{Value}' [{StartIndex}..{EndIndex}]";
}
