using AdventureGame.Engine.Actions;
using AdventureGame.Engine.Conditions;
using AdventureGame.Engine.Plugins;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AdventureGame.Engine.DependencyInjection;

/// <summary>
/// Provides DI registration for all game engine components,
/// automatically discovering Actions and Condition Operators
/// across all assemblies marked with [AdventureGameAssembly].
/// </summary>
public static class GameElementServiceExtensions
{
    /// <summary>
    /// Registers all engine services, including auto-discovered
    /// effect actions, condition operators, and supporting catalogs.
    /// </summary>
    public static IServiceCollection AddGameElementDependencies(this IServiceCollection services)
    {
        // Discover and register types
        RegisterEffectActions(services);
        RegisterConditionOperators(services);

        // Catalogs (receive IEnumerable<T> from DI)
        services.AddSingleton<IEffectActionCatalog, EffectActionCatalog>();
        services.AddSingleton<IConditionOperatorCatalog, ConditionOperatorCatalog>();

        // Evaluator
        services.AddTransient<ConditionEvaluator>();

        return services;
    }

    // ------------------------------------------------------------
    // Registration Helpers
    // ------------------------------------------------------------

    private static void RegisterEffectActions(IServiceCollection services)
    {
        var baseType = typeof(IEffectAction);

        foreach (var asm in GetAssembliesToScan())
        {
            var implementations = asm.GetTypes()
                .Where(t => baseType.IsAssignableFrom(t)
                         && t.IsClass && !t.IsAbstract);

            foreach (var impl in implementations)
                services.AddTransient(baseType, impl);
        }
    }

    private static void RegisterConditionOperators(IServiceCollection services)
    {
        var baseType = typeof(IConditionOperator);

        foreach (var asm in GetAssembliesToScan())
        {
            var implementations = asm.GetTypes()
                .Where(t => baseType.IsAssignableFrom(t)
                         && t.IsClass && !t.IsAbstract);

            foreach (var impl in implementations)
                services.AddTransient(baseType, impl);
        }
    }

    // ------------------------------------------------------------
    // Assembly Discovery (Engine + Plugins)
    // ------------------------------------------------------------

    private static IEnumerable<Assembly> GetAssembliesToScan()
    {
        // Scan assemblies currently loaded that are marked with the attribute
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.IsDynamic)
                continue;

            if (asm.GetCustomAttribute<AdventureGameAssemblyAttribute>() != null)
                yield return asm;
        }
    }
}
