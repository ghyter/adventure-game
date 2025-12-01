namespace AdventureGame.Engine.Parameters;

/// <summary>
/// Default implementation of the parameter catalog.
/// Discovers all IParameterTypeHandler instances from dependency injection.
/// </summary>
public sealed class ParameterCatalog(IEnumerable<IParameterTypeHandler> handlers) : IParameterCatalog
{
    private readonly Dictionary<string, IParameterTypeHandler> _handlers = handlers.ToDictionary(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);

    public IEnumerable<IParameterTypeHandler> All => _handlers.Values;

    public IParameterTypeHandler? Get(string key) =>
        _handlers.TryGetValue(key, out var h) ? h : null;
}
