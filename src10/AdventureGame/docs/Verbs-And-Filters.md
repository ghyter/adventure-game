# Copilot Implementation Instructions --- Verbs, Filters, Triggers, Command Parser, Verb Tester, and Editor

## 1. Create a reusable **GameElementFilter**

### File:

`AdventureGame.Engine/Filters/GameElementFilter.cs`

``` csharp
namespace AdventureGame.Engine.Filters;

public enum GameElementFilterMode {
    All,
    Types,
    Tags,
    Names
}

public class GameElementFilter {
    public GameElementFilterMode Mode { get; set; } = GameElementFilterMode.All;
    public List<string> Types { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Names { get; set; } = new();

    public bool Matches(GameElement element) {
        switch (Mode) {
            case GameElementFilterMode.All:
                return true;
            case GameElementFilterMode.Types:
                return Types.Contains(element.Kind);
            case GameElementFilterMode.Tags:
                return element.Tags.Any(t => Tags.Contains(t));
            case GameElementFilterMode.Names:
                return Names.Contains(element.Name, StringComparer.OrdinalIgnoreCase)
                    || element.Aliases.Any(a => Names.Contains(a, StringComparer.OrdinalIgnoreCase));
            default:
                return false;
        }
    }

    public int Score(GameElement element) {
        if (!Matches(element)) return 0;

        return Mode switch {
            GameElementFilterMode.All => 10,
            GameElementFilterMode.Types => 50,
            GameElementFilterMode.Tags => 80,
            GameElementFilterMode.Names => 100,
            _ => 0
        };
    }
}
```

------------------------------------------------------------------------

## 2. Verb Target Rules

### File:

`AdventureGame.Engine/Verbs/Verb.cs`

``` csharp
public class Verb {
    public string Name { get; set; } = "";
    public List<string> Aliases { get; set; } = new();
    public List<string> Tags { get; set; } = new();

    public GameElementFilter Target1 { get; set; } = new GameElementFilter();
    public GameElementFilter Target2 { get; set; } = new GameElementFilter();

    public List<string> ConditionTexts { get; set; } = new();
    public List<VerbEffect> Effects { get; set; } = new();
}
```

------------------------------------------------------------------------

## 3. VerbEffect

### File:

`AdventureGame.Engine/Verbs/VerbEffect.cs`

``` csharp
public class VerbEffect {
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 0;
    public string SuccessText { get; set; } = "";
    public string FailureText { get; set; } = "";
    public string Action { get; set; } = "";
}
```

Rules:\
- Min/Max 0/0 = always succeed\
- Otherwise roll a d20\
- 1 = automatic fail\
- 20 = automatic success\
- Overlapping effects are allowed

------------------------------------------------------------------------

## 4. Triggers

### File:

`AdventureGame.Engine/Triggers/Trigger.cs`

``` csharp
public class Trigger {
    public string Name { get; set; } = "";
    public List<string> ConditionTexts { get; set; } = new();
    public List<VerbEffect> Effects { get; set; } = new();

    public bool FiredThisRound { get; set; } = false;
}
```

**Execution rules:**\
- After each round, try to fire triggers whose conditions are met\
- A trigger fires at most once per round\
- Trigger effects can cause other triggers to become valid

------------------------------------------------------------------------

## 5. VerbResolver

### File:

`AdventureGame.Engine/Verbs/VerbResolver.cs`

``` csharp
public class VerbResolver {
    public Verb? ResolveVerb(
        string verbToken,
        GameElement? target1,
        GameElement? target2,
        IEnumerable<Verb> verbs)
    {
        var candidates = verbs.Where(v =>
               v.Name.Equals(verbToken, StringComparison.OrdinalIgnoreCase)
            || v.Aliases.Contains(verbToken, StringComparer.OrdinalIgnoreCase));

        var scored = new List<(Verb verb, int score)>();

        foreach (var v in candidates) {
            int score = 0;
            if (target1 != null) score += v.Target1.Score(target1);
            if (target2 != null) score += v.Target2.Score(target2);
            scored.Add((v, score));
        }

        return scored.OrderByDescending(s => s.score)
                     .Select(s => s.verb)
                     .FirstOrDefault();
    }
}
```

------------------------------------------------------------------------

## 6. CommandParser

### File:

`AdventureGame.Engine/Parser/CommandParser.cs`

Rules:\
- Remove fluff words unless part of a name/alias\
- Parse format:\
- verb\
- verb target\
- verb target1 target2\
- Resolve directional words into Exit elements\
- Resolve names/aliases/tags dynamically against the session (not IDs)

------------------------------------------------------------------------

## 7. Describe Action

### File:

`AdventureGame.Engine/Actions/DescribeAction.cs`

``` csharp
public class ActionExecutor {
    public string ExecuteDescribe(GameElement element) {
        return $"{element.Name}
{element.Description}
{element.CurrentState.Description}";
    }
}
```

Supports:\
- "describe currentScene"\
- "describe target"

------------------------------------------------------------------------

## 8. Verb Editor Page (Blazor)

### File:

`AdventureGame/Pages/VerbEditor.razor`

Requirements: - Single non-tab layout\
- Fields:\
- Name\
- Aliases (RadzenChips)\
- Tags (RadzenChips)\
- Conditions list\
- GameElementFilterControl for Target1 & Target2\
- Effects editor (Min, Max, Success, Failure, Action)

### Component:

`AdventureGame/Shared/GameElementFilterControl.razor`

Supports: - Mode (All, Types, Tags, Names) - Multi-type selection -
Multi-tag selection - Multi-name selection

------------------------------------------------------------------------

## 9. Verb Tester Page

### File:

`AdventureGame/Pages/VerbTester.razor`

Features: - Sandbox session\
- Raw command input\
- Display:\
- verb token\
- target1\
- target2\
- selected verb\
- condition evaluation\
- effect outcome

Uses: - CommandParser\
- VerbResolver\
- ConditionEngine\
- EffectEngine

------------------------------------------------------------------------

## 10. Required Files Summary

    AdventureGame.Engine/Filters/GameElementFilter.cs
    AdventureGame.Engine/Verbs/Verb.cs
    AdventureGame.Engine/Verbs/VerbEffect.cs
    AdventureGame.Engine/Verbs/VerbResolver.cs
    AdventureGame.Engine/Triggers/Trigger.cs
    AdventureGame.Engine/Parser/CommandParser.cs
    AdventureGame.Engine/Actions/DescribeAction.cs

    AdventureGame/Shared/GameElementFilterControl.razor
    AdventureGame/Pages/VerbEditor.razor
    AdventureGame/Pages/VerbTester.razor

    AdventureGame/Documentation/Copilot-Verbs-And-Filters.md

------------------------------------------------------------------------

## Copilot Command

Tell Copilot:

> "Implement everything described in
> `AdventureGame/Documentation/Copilot-Verbs-And-Filters.md`."
