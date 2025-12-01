namespace AdventureGame.Engine.Effects;

/// <summary>
/// Default implementation of the effect action catalog.
/// Discovers all IEffectAction instances from dependency injection.
/// </summary>
public sealed class EffectActionCatalog : IEffectActionCatalog
{
    private readonly IReadOnlyList<IEffectAction> _actions;
    private readonly Dictionary<string, IEffectAction> _actionsByKey;

    public EffectActionCatalog(IEnumerable<IEffectAction> actions)
    {
        _actions = actions.ToList();
        _actionsByKey = _actions.ToDictionary(a => a.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IEffectAction> All => _actions;

    public IEffectAction? GetByKey(string key)
    {
        return _actionsByKey.TryGetValue(key, out var action) ? action : null;
    }
}
