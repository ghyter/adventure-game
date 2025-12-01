using AdventureGame.Engine.Actions;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Engine.Actions.Implementations;

/// <summary>
/// Sets a key/value pair inside one of a GameElement's dictionaries:
/// Flags (bool), Properties (string), or Attributes (int).
/// </summary>
public sealed class SetDictionaryValueEffect : IEffectAction
{
    public string Key => "setDictionaryValue";

    public string DisplayName => "Set Dictionary Value";

    public string Description =>
        "Sets a value in one of the target's dictionaries (Flags, Properties, Attributes). " +
        "Automatically converts the value to the correct type.";

    public IReadOnlyList<EffectParameterDescriptor> Parameters { get; } =
        new[]
        {
            new EffectParameterDescriptor
            {
                Name = "dictionary",
                Description = "Which dictionary to modify: flags, properties, or attributes.",
                IsRequired = true
            },
            new EffectParameterDescriptor
            {
                Name = "key",
                Description = "The dictionary key to modify.",
                IsRequired = true
            },
            new EffectParameterDescriptor
            {
                Name = "value",
                Description = "The value to assign (bool/int/string depending on dictionary).",
                IsRequired = true
            },
            new EffectParameterDescriptor
            {
                Name = "target",
                Description = "The GameElement to modify. Usually 'target' or 'target2' from a verb.",
                IsRequired = true
            }
        };

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        // ------------------------------
        // Validate params
        // ------------------------------
        if (!parameters.TryGetValue("target", out var targetId))
            throw new InvalidOperationException("SetDictionaryValueEffect missing 'target' parameter.");

        if (!parameters.TryGetValue("dictionary", out var dict))
            throw new InvalidOperationException("SetDictionaryValueEffect missing 'dictionary' parameter.");

        if (!parameters.TryGetValue("key", out var key))
            throw new InvalidOperationException("SetDictionaryValueEffect missing 'key' parameter.");

        if (!parameters.TryGetValue("value", out var rawValue))
            throw new InvalidOperationException("SetDictionaryValueEffect missing 'value' parameter.");

        GameElement? target = null;
        if (round.Target1 != null && round.Target1.Id == targetId)
        {
            target = round.Target1;
        }
        else if (round.Target2 != null && round.Target2.Id == targetId)
        {
            target = round.Target2;
        }

        if (target == null)
            throw new InvalidOperationException($"Target '{targetId}' not found in round.");

        // Normalize dictionary selector
        dict = dict.Trim().ToLowerInvariant();

        // ------------------------------
        // Apply mutation to round
        // ------------------------------
        switch (dict)
        {
            case "flags":
                ApplyFlag(target, key, rawValue, round);
                break;

            case "properties":
                ApplyProperty(target, key, rawValue, round);
                break;

            case "attributes":
                ApplyAttribute(target, key, rawValue, round);
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid dictionary '{dict}'. Expected 'flags', 'properties', or 'attributes'.");
        }

        return Task.CompletedTask;
    }

    // ----------------------------------------------------------------------
    // Handlers (write to the Round's mutated state, not session directly)
    // ----------------------------------------------------------------------

    private static void ApplyFlag(GameElement element, string key, string rawValue, GameRound round)
    {
        if (!bool.TryParse(rawValue, out var b))
            throw new InvalidOperationException($"Flag '{key}' must be true/false.");

        element.Flags[key] = b;
    }

    private static void ApplyProperty(GameElement element, string key, string rawValue, GameRound round)
    {
        // Properties are strings
        element.Properties[key] = rawValue;
    }

    private static void ApplyAttribute(GameElement element, string key, string rawValue, GameRound round)
    {
        if (!int.TryParse(rawValue, out var i))
            throw new InvalidOperationException($"Attribute '{key}' must be an integer.");

        element.Attributes[key] = i;
    }
}
