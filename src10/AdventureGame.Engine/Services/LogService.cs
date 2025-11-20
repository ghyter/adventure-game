namespace AdventureGame.Engine.Services;

/// <summary>
/// Captures console output and broadcasts log messages to subscribers.
/// </summary>
public class LogService
{
    private readonly List<string> _logHistory = [];
    private readonly int _maxHistory = 1000;
    private static LogService? _instance;

    public event EventHandler<LogEventArgs>? LogReceived;

    public IReadOnlyList<string> LogHistory => _logHistory.AsReadOnly();

    public LogService()
    {
        _instance = this;
        // Hook into debug output if available
        SetupDebugOutputCapture();
    }

    public void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var formattedMessage = $"[{timestamp}] {message}";
        
        _logHistory.Add(formattedMessage);
        
        // Trim history if it exceeds max size
        if (_logHistory.Count > _maxHistory)
        {
            _logHistory.RemoveRange(0, _logHistory.Count - _maxHistory);
        }

        LogReceived?.Invoke(this, new LogEventArgs(formattedMessage));
    }

    public void Clear()
    {
        _logHistory.Clear();
    }

    private void SetupDebugOutputCapture()
    {
        // Create a custom TextWriter to capture Console.Out and Console.Error
        var writer = new LogCapturingTextWriter(this);

        try
        {
            // Redirect Console.Out to our capturing writer
            Console.SetOut(writer);
            Console.SetError(writer);
        }
        catch (Exception ex)
        {
            // If we can't redirect, log it
            Log($"WARNING: Could not redirect console: {ex.Message}");
        }
    }

    /// <summary>
    /// Static helper to log from anywhere without needing DI
    /// </summary>
    internal static void LogStatic(string message)
    {
        _instance?.Log(message);
    }
}

public class LogEventArgs(string message) : EventArgs
{
    public string Message { get; } = message;
}

/// <summary>
/// Custom TextWriter that captures writes and sends them to LogService.
/// </summary>
internal class LogCapturingTextWriter(LogService logService) : System.IO.TextWriter
{
    private readonly LogService _logService = logService;
    private string _buffer = "";

    public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

    public override void Write(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _logService.Log(value);
        }
    }

    public override void WriteLine(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _logService.Log(value);
        }
        else
        {
            _logService.Log("");
        }
    }

    public override void Write(char value)
    {
        _logService.Log(value.ToString());
    }

    public override void WriteLine()
    {
        _logService.Log("");
    }
}
