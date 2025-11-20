namespace AdventureGame.Engine.DSL;

using AdventureGame.Engine.DSL.AST;
using System.Threading;

/// <summary>
/// Thread-safe cache for compiled DSL condition expressions.
/// Keyed by canonical text representation.
/// </summary>
public sealed class CompiledExpressionCache
{
    private readonly Dictionary<string, ConditionNode> _conditionCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Lock _lock = new();

    /// <summary>
    /// Gets a cached condition or compiles and caches a new one.
    /// </summary>
    /// <param name="key">Canonical DSL text key</param>
    /// <param name="compiler">Function to compile the condition if not cached</param>
    /// <returns>The cached or newly compiled ConditionNode, or null if compilation failed</returns>
    public ConditionNode? GetOrAddCondition(string key, Func<string, ConditionNode?> compiler)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        if (compiler == null) throw new ArgumentNullException(nameof(compiler));

        lock (_lock)
        {
            if (_conditionCache.TryGetValue(key, out var node))
            {
                return node;
            }

            var compiled = compiler(key);
            if (compiled != null)
            {
                _conditionCache[key] = compiled;
            }

            return compiled;
        }
    }

    /// <summary>
    /// Clears all cached entries.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _conditionCache.Clear();
        }
    }

    /// <summary>
    /// Gets the current number of cached entries.
    /// </summary>
    public int CacheSize
    {
        get
        {
            lock (_lock)
            {
                return _conditionCache.Count;
            }
        }
    }
}
