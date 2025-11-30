using AdventureGame.Engine.Actions;
using AdventureGame.Engine.Actions.Implementations;
using AdventureGame.Engine.Conditions;
using AdventureGame.Engine.Conditions.Implementations;

namespace AdventureGame.DependencyInjection;

/// <summary>
/// Extension methods for registering game element services in dependency injection.
/// This provides a single unified extension point for all GameEngine logic.
/// </summary>
public static class GameElementServiceExtensions
{
    /// <summary>
    /// Registers all game element dependencies including effect actions and condition operators.
    /// This method should be called from MauiProgram.cs instead of registering services individually.
    /// </summary>
    /// <param name="builder">The MAUI app builder</param>
    /// <returns>The builder for chaining</returns>
    public static MauiAppBuilder AddGameElementDependencies(this MauiAppBuilder builder)
    {
        var services = builder.Services;

        // Register Effect Actions (auto-discovered by catalog)
        services.AddTransient<IEffectAction, PrintEffect>();
        services.AddTransient<IEffectAction, RollDiceEffect>();
        // Add additional effect actions here as they are created...

        // Register Effect Action Catalog
        services.AddSingleton<IEffectActionCatalog, EffectActionCatalog>();

        // Register Condition Operators
        services.AddTransient<IConditionOperator, EqualsOperator>();
        services.AddTransient<IConditionOperator, DiceCheckOperator>();
        services.AddTransient<IConditionOperator, PercentageChanceOperator>();
        // Add additional condition operators here as they are created...

        // Register Condition Operator Catalog
        services.AddSingleton<IConditionOperatorCatalog, ConditionOperatorCatalog>();

        // Register Condition Evaluator
        services.AddTransient<ConditionEvaluator>();

        return builder;
    }
}
