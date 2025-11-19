namespace AdventureGame.Engine.DSL.Parser;

using AdventureGame.Engine.DSL.Tokenizer;
using AdventureGame.Engine.DSL.AST;

/// <summary>
/// Represents a parsing error with position information.
/// </summary>
public class DslError(string message, int startIndex, int endIndex, ErrorSeverity severity = ErrorSeverity.Error)
{
    public string Message { get; set; } = message;
    public int StartIndex { get; set; } = startIndex;
    public int EndIndex { get; set; } = endIndex;
    public ErrorSeverity Severity { get; set; } = severity;

    public override string ToString() => $"[{Severity}] {Message} [{StartIndex}..{EndIndex}]";
}

public enum ErrorSeverity
{
    Warning,
    Error
}

/// <summary>
/// Result of parsing/validating a DSL expression.
/// </summary>
public class DslParseResult
{
    public bool Success { get; set; }
    public ConditionNode? Ast { get; set; }
    public List<DslError> Errors { get; set; } = [];

    public void AddError(string message, int startIndex, int endIndex, ErrorSeverity severity = ErrorSeverity.Error)
    {
        Errors.Add(new DslError(message, startIndex, endIndex, severity));
        if (severity == ErrorSeverity.Error)
        {
            Success = false;
        }
    }

    public override string ToString() => Success ? "Parse successful" : $"Parse failed with {Errors.Count} error(s)";
}
