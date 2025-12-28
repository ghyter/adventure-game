namespace AdventureGame.Engine.Extensions;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(
        this IDictionary<TKey, TValue> dict,
        IDictionary<TKey, TValue> source)
    {
        foreach (var (key, value) in source)
            dict[key] = value;
    }
}