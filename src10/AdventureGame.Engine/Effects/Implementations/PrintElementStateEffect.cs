using AdventureGame.Engine.Models;
using AdventureGame.Engine.Models.Actions;
using AdventureGame.Engine.Models.Runtime;
using AdventureGame.Engine.Parameters;

namespace AdventureGame.Engine.Effects.Implementations;

/// <summary>
/// Prints the current state description of a game element.
/// Outputs the element's name, description, and current state text.
/// Useful for "look" commands and room entry descriptions.
/// </summary>
public sealed class PrintElementStateEffect : IEffectAction
{
    public string Key => "print_element_state";

    public string DisplayName => "Print Element State";

    public string Description => "Prints an element's name, description, and current state text";

    public IReadOnlyList<ParameterDescriptor> Parameters { get; } =
    [
        new()
        {
            Name = "element",
            DisplayName = "Element",
            ParameterType = "gameElement",
            IsOptional = false,
            Description = "The element to describe (name, alias, or special variable: target, target1, target2, currentScene, currentPlayer)"
        },
        new()
        {
            Name = "outputMode",
            DisplayName = "Output Mode",
            ParameterType = "outputMode",
            IsOptional = true,
            DefaultValue = "Both",
            Description = "What to include: Description, State, or Both"
        },
        new()
        {
            Name = "format",
            DisplayName = "Format Template",
            ParameterType = "string",
            IsOptional = true,
            DefaultValue = "{name}\n{description}\n{state}",
            Description = "Template for output. Available placeholders: {name}, {description}, {state}, {stateName}"
        }
    ];

    public Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("element", out var elementRef))
        {
            throw new InvalidOperationException("PrintElementStateEffect requires 'element' parameter");
        }

        // Resolve element reference to actual element
        var element = ElementReferenceResolver.ResolveElement(elementRef, round, session);
        if (element == null)
        {
            throw new InvalidOperationException($"Element '{elementRef}' not found");
        }

        // Get output mode (Description, State, or Both)
        var outputMode = parameters.TryGetValue("outputMode", out var modeStr) && !string.IsNullOrWhiteSpace(modeStr)
            ? modeStr
            : "Both";

        var format = parameters.TryGetValue("format", out var formatStr) && !string.IsNullOrWhiteSpace(formatStr)
            ? formatStr
            : GetDefaultFormat(outputMode);

        // Build the output text
        var output = BuildOutput(element, outputMode, format);

        // Add to round output
        round.Output.Add(output);

        return Task.CompletedTask;
    }

    private static string GetDefaultFormat(string outputMode)
    {
        return outputMode.ToLowerInvariant() switch
        {
            "description" => "{name}\n{description}",
            "state" => "{state}",
            "both" => "{name}\n{description}\n{state}",
            _ => "{name}\n{description}\n{state}"
        };
    }

    private static string BuildOutput(GameElement element, string outputMode, string format)
    {
        // Get current state text
        var stateText = "";
        var stateName = element.DefaultState ?? "default";

        if (element.States.TryGetValue(stateName, out var state))
        {
            stateText = state.Description ?? "";
        }

        // Determine what to include based on output mode
        var description = element.Description ?? "";
        var mode = outputMode.ToLowerInvariant();

        if (mode == "state")
        {
            description = ""; // Don't include description
        }
        else if (mode == "description")
        {
            stateText = ""; // Don't include state
        }

        // Replace placeholders in format template
        var output = format
            .Replace("{name}", element.Name)
            .Replace("{description}", description)
            .Replace("{state}", stateText)
            .Replace("{stateName}", stateName)
            .Replace("\\n", "\n")  // Support escaped newlines
            .Replace("\\t", "\t"); // Support escaped tabs

        return output;
    }
}
