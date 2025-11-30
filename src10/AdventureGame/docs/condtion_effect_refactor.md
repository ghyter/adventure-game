
# Effect & Condition System Rewrite Instructions (For CoPilot)

## Overview
This document describes the complete plan for replacing the existing effect and condition system in the AdventureGame engine.  
It provides explicit instructions for **removing old logic**, **installing the new extensible architecture**, and **adding metadata-driven operators/actions** suitable for editor UI integration.

---

# 1. REMOVE OLD LOGIC

Delete or deprecate ALL of the following:

- Any existing DSL parsers for effects or conditions  
- Any AST-related classes previously used for commands  
- Old condition/effect handlers that rely on:
  - implicit target detection
  - hard-coded verbs
  - natural-language parsing
  - unstructured dictionaries
- Condition evaluation based on string parsing  
- Effect execution based on:
  - property strings with dot access
  - switch statements
  - Type-based effect classes that cannot be discovered dynamically  
- Any parsers that attempted to convert text into effects or conditions  

**All condition/effect logic MUST be replaced with catalog-based plugins described below.**

---

# 2. EFFECT SYSTEM (NEW ARCHITECTURE)

## 2.1 Create IEffectAction
Each effect is a discoverable action, with metadata for the UI.

```csharp
public interface IEffectAction
{
    string Key { get; }
    string DisplayName { get; }
    string Description { get; }
    IReadOnlyList<EffectParameterDescriptor> Parameters { get; }

    Task ExecuteAsync(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters);
}
```

---

## 2.2 Create EffectParameterDescriptor
```csharp
public sealed class EffectParameterDescriptor
{
    public string Name { get; set; } = "";
    public EffectParameterKind Kind { get; set; }
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
}
```

Parameter kinds should include:

```
Target
PropertyName
AttributeName
Value
Number
Boolean
Location
Exit
Group
DiceExpression
```

---

## 2.3 Create EffectActionCatalog
Responsible for discovering all actions via DI.

```csharp
public interface IEffectActionCatalog
{
    IReadOnlyList<IEffectAction> All { get; }
    IEffectAction? GetByKey(string key);
}
```

Register in DI:

```csharp
services.AddTransient<IEffectAction, SetPropertyEffect>();
services.AddTransient<IEffectAction, MoveEffect>();
services.AddTransient<IEffectAction, PrintEffect>();
services.AddSingleton<IEffectActionCatalog, EffectActionCatalog>();
```

---

## 2.4 GameRound Extensions
Effects always receive:

- GameRound (logs, debug info, targets)
- GameSession (world, resolver, player, currentScene)

**Effects should mutate the round, not the session, until commit.**

---

# 3. CONDITION SYSTEM (NEW ARCHITECTURE)

## 3.1 Create IConditionOperator
```csharp
public interface IConditionOperator
{
    string Key { get; }
    string DisplayName { get; }
    string Description { get; }
    IReadOnlyList<ConditionParameterDescriptor> Parameters { get; }

    bool Evaluate(
        GameRound round,
        GameSession session,
        IReadOnlyDictionary<string, string> parameters);
}
```

---

## 3.2 ConditionParameterDescriptor
```csharp
public sealed class ConditionParameterDescriptor
{
    public string Name { get; set; } = "";
    public ConditionParameterKind Kind { get; set; }
    public bool IsRequired { get; set; }
    public string? Description { get; set; }
}
```

---

## 3.3 ConditionOperatorCatalog
Exactly like effect catalog.

---

## 3.4 Condition Blocks (AND/OR/NOT)
Create:

```csharp
public sealed class ConditionDefinition
{
    public string OperatorKey { get; set; } = "";
    public Dictionary<string,string> Parameters { get; set; } = new();
}

public sealed class ConditionBlock
{
    public List<ConditionDefinition> AllOf { get; set; } = new();
    public List<ConditionDefinition> AnyOf { get; set; } = new();
    public List<ConditionDefinition> NoneOf { get; set; } = new();
}
```

---

## 3.5 ConditionEvaluator
Handles nested evaluation.

---

# 4. DICE SYSTEM INTEGRATION

## 4.1 Keep existing DiceHelper
BUT extend with new parsing logic:

- Multi-group expressions (`2d6+1d4+3`)
- Negative modifiers
- Advantage/disadvantage
- Return `RollResult`

Create:

```csharp
public static class DiceHelperExtensions
{
    RollResult RollExpression(string expr, int threshold = 0)
}
```

---

## 4.2 Add dice-related ConditionOperators
Examples:

### diceCheck
Parameters: diceExpression, threshold  
Return total >= threshold.

### percentageChance
Rolling 1d100 <= percent.

### skillDiceCheck
RollExpression + attribute bonus.

---

## 4.3 Add dice-based EffectActions

### rollDice  
Writes result to round log.

### rollDiceAndStore  
Stores Total into a target property.

### rollDiceDifficulty  
Outputs pass/fail based on threshold.

---

# 5. GAME ROUND + SESSION MODEL

### GameRound:
- Target1/Target2
- Verb
- Output log
- Debug roll log
- Pending changes

### GameSession:
- World state
- Elements
- Player
- CurrentScene
- Resolver
- Global stats

Effects/Conditions ALWAYS receive:

```csharp
ExecuteAsync(GameRound round, GameSession session, parameters)
Evaluate(GameRound round, GameSession session, parameters)
```

---

# 6. REMOVE ALL LEGACY SYSTEMS

Delete:

- Old Condition classes
- Old Effect classes
- AST nodes
- DSL grammar files for effects/conditions
- Natural language parsing for effects/conditions
- Switch-case based processors
- Dot-notation property resolvers tied to the old system

---

# 7. UI REQUIREMENTS

The editor must:

1. Query catalogs:
   - IEffectActionCatalog
   - IConditionOperatorCatalog

2. Populate dropdowns with:
   - DisplayName
   - Description

3. Auto-generate parameter fields based on descriptor.Kind

4. Support building ConditionBlocks:
   - AllOf
   - AnyOf
   - NoneOf

5. Support dice expression input fields

---

# 8. FINAL NOTES FOR COPILOT

- Use dependency injection for all catalogs  
- Avoid hardcoded logic  
- All effects/conditions MUST be discoverable  
- Dice logic must be reusable by both effects and conditions  
- Only commit world-state changes after round completes  
- Ensure all logs go into GameRound for UI rendering  

---

---

# 9. REGISTRATION VIA EXTENSION METHOD (NOT IN MauiProgram.cs)

## 9.1 Create a dedicated extension method for dependency registration

Instead of registering effect actions and condition operators inside `MauiProgram.cs`,  
create a new static class such as:

```csharp
public static class GameElementServiceExtensions
{
    public static MauiAppBuilder AddGameElementDependencies(this MauiAppBuilder builder)
    {
        var services = builder.Services;

        // Register Effect Actions (auto-discovered)
        services.AddTransient<IEffectAction, SetPropertyEffect>();
        services.AddTransient<IEffectAction, MoveEffect>();
        services.AddTransient<IEffectAction, PrintEffect>();
        services.AddTransient<IEffectAction, RollDiceEffect>();
        // Add additional effect actions here...

        services.AddSingleton<IEffectActionCatalog, EffectActionCatalog>();

        // Register Condition Operators
        services.AddTransient<IConditionOperator, EqualsOperator>();
        services.AddTransient<IConditionOperator, ContainsOperator>();
        services.AddTransient<IConditionOperator, DiceCheckOperator>();
        services.AddTransient<IConditionOperator, SkillDiceCheckOperator>();
        // Add additional condition operators here...

        services.AddSingleton<IConditionOperatorCatalog, ConditionOperatorCatalog>();

        return builder;
    }
}
```

This creates a **single unified extension point** for all GameEngine logic.

---

## 9.2 Update MauiProgram.cs to use the extension method

Replace all effect/condition registrations in `MauiProgram.cs` with:

```csharp
builder.AddGameElementDependencies();
```

This guarantees:

- MauiProgram.cs remains clean and readable  
- Game engine logic is encapsulated  
- Copilot can inject new operators/actions without clutter  
- Future developers always know where engine registration lives  

---

## 9.3 Guideline for future additions

Whenever a new EffectAction or ConditionOperator is created:

1. Add the class file  
2. Register it inside `AddGameElementDependencies()`  
3. Rebuild the catalogs automatically via DI  
4. The editor will immediately discover the new operator/action  

Never add DI registrations directly in MauiProgram.cs.

---

# END OF EXTENSION METHOD INSTRUCTIONS
