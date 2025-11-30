namespace AdventureGame.Engine.Conditions;

/// <summary>
/// Default implementation of the condition operator catalog.
/// Discovers all IConditionOperator instances from dependency injection.
/// </summary>
public sealed class ConditionOperatorCatalog : IConditionOperatorCatalog
{
    private readonly IReadOnlyList<IConditionOperator> _operators;
    private readonly Dictionary<string, IConditionOperator> _operatorsByKey;

    public ConditionOperatorCatalog(IEnumerable<IConditionOperator> operators)
    {
        _operators = operators.ToList();
        _operatorsByKey = _operators.ToDictionary(o => o.Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IConditionOperator> All => _operators;

    public IConditionOperator? GetByKey(string key)
    {
        return _operatorsByKey.TryGetValue(key, out var op) ? op : null;
    }
}
