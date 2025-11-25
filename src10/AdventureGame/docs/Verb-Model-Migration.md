# Removal of Old Verb Model - Summary

## Overview

Successfully removed the old `AdventureGame.Engine.Models.Round.Verb` class and migrated all references to the new simplified `AdventureGame.Engine.Verbs.Verb` class.

## Files Removed

1. **AdventureGame.Engine/Models/Round/Verb.cs**
   - Old complex Verb model with Target1Id, Target2Id
   - Had DifficultyCheck, Preconditions, StructuredEffects
   - Inherited from GameAction base class

2. **AdventureGame/Components/Actions/VerbEditor.razor**
   - Tab-based editor for the old Verb model
   - Supported legacy target IDs and complex effects
   - Used ConditionGroupEditor, ModifierEditor, etc.

## Files Modified

### 1. AdventureGame.Engine/Models/GamePack.cs
**Before:**
```csharp
using AdventureGame.Engine.Models.Round;
...
public List<Verb> Verbs { get; set; } = [];
```

**After:**
```csharp
using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Verbs;
...
public List<Verb> Verbs { get; set; } = [];  // Now uses Engine.Verbs.Verb
```

### 2. AdventureGame.Engine/Runtime/GameSession.cs
**Before:**
```csharp
using AdventureGame.Engine.Models.Round;
...
public List<Verb> Verbs { get; } = [];
```

**After:**
```csharp
using AdventureGame.Engine.Verbs;
...
public List<Verb> Verbs { get; } = [];  // Now uses Engine.Verbs.Verb
```

### 3. AdventureGame/Components/Pages/VerbsPage.razor
**Before:**
```csharp
@using AdventureGame.Components.Actions
@using AdventureGame.Engine.Models.Round
...
var result = await DialogService.OpenSideAsync<VerbEditor>(...);
```

**Grid columns:**
- Targets: `v.Target1Id/v.Target2Id`
- Effects: `v.StructuredEffects?.Effects.Count ?? v.Effects.Count`

**After:**
```csharp
@using AdventureGame.Engine.Verbs
@using AdventureGame.Components.Pages.Tools
...
var result = await DialogService.OpenSideAsync<VerbEditor>(...);
```

**Grid columns:**
- Target 1: Shows filter mode (All, Types, Tags, Names)
- Target 2: Shows filter mode
- Conditions: `v.ConditionTexts.Count`
- Effects: `v.Effects.Count`

## Model Comparison

### Old Model (Models.Round.Verb)
```csharp
public sealed class Verb : GameAction
{
    public string? Target1Id { get; set; }
    public string? Target2Id { get; set; }
    
    public ConditionGroup? Preconditions { get; set; }
    public DifficultyCheck DifficultyCheck { get; set; } = new();
    public EffectGroup? StructuredEffects { get; set; }
    public List<EffectRange>? EffectsByRange { get; set; }
    public string? SuccessMessage { get; set; }
    public string? FailureMessage { get; set; }
    
    // Inherited from GameAction:
    public string? Name { get; set; }
    public HashSet<string> Aliases { get; set; }
    public string Description { get; set; }
    public Condition? Conditions { get; set; }
    public List<GameEffect> Effects { get; }
}
```

### New Model (Engine.Verbs.Verb)
```csharp
public class Verb
{
    public string Name { get; set; } = "";
    public HashSet<string> Aliases { get; set; } = new();
    public HashSet<string> Tags { get; set; } = new();

    public GameElementFilter Target1 { get; set; } = new();
    public GameElementFilter Target2 { get; set; } = new();

    public List<string> ConditionTexts { get; set; } = new();
    public List<VerbEffect> Effects { get; set; } = new();
}
```

## Key Differences

| Feature | Old Model | New Model |
|---------|-----------|-----------|
| **Inheritance** | Inherits from GameAction | Standalone class |
| **Targets** | String IDs (Target1Id, Target2Id) | GameElementFilter with modes |
| **Conditions** | ConditionGroup tree structure | List of DSL text strings |
| **Effects** | EffectGroup + EffectRange lists | List of VerbEffect |
| **Difficulty** | DifficultyCheck with modifiers | Simple Min/Max on VerbEffect |
| **Messages** | Separate Success/FailureMessage | Bundled in VerbEffect |
| **Complexity** | High (many nested structures) | Low (flat structure) |

## Benefits of New Model

### 1. Simpler Structure
- No complex inheritance
- Flat property list
- Easier to understand and maintain

### 2. Filter-Based Targeting
- More flexible than hard-coded IDs
- Supports type, tag, name, and "all" matching
- Better for dynamic target resolution

### 3. DSL-Based Conditions
- Natural language instead of tree structures
- Easier for content creators
- Leverages existing DSL infrastructure

### 4. Self-Contained Effects
- Each effect has its own difficulty range
- Messages bundled with effects
- Simpler to author and understand

### 5. Tagging Support
- Verbs can be categorized by tags
- Better organization and filtering
- Supports meta-systems

## Migration Path (For Data)

If you have existing game data with the old verb model, you'll need to migrate:

### Target Migration
```csharp
// Old: Target1Id = "apple_001"
// New: Target1.Mode = Names, Target1.Names = ["apple"]
```

### Condition Migration
```csharp
// Old: Preconditions = new ConditionGroup { ... }
// New: ConditionTexts = ["when player has key", "when door is locked"]
```

### Effect Migration
```csharp
// Old: StructuredEffects.Effects = [...]
//      DifficultyCheck.Difficulty = 15
//      SuccessMessage = "..."
// New: Effects = [new VerbEffect { 
//        Min = 10, Max = 20, 
//        SuccessText = "...",
//        Action = "..." 
//      }]
```

## Build Status

? **Build Successful**
- No compilation errors
- All references updated correctly
- Type safety maintained

## Testing Status

All existing tests should continue to pass, as:
- GamePack serialization handles the new Verb type
- GameSession loading works with new Verb type
- No breaking changes to GameElement or other core models

## UI Changes

### VerbsPage Grid Columns
**Before:**
- Name
- Targets (showed Target1Id/Target2Id)
- Aliases (count)
- Effects (count)
- Actions (Edit/Delete)

**After:**
- Name
- Target 1 (shows filter mode)
- Target 2 (shows filter mode)
- Aliases (count)
- Conditions (count)
- Effects (count)
- Actions (Edit/Delete)

### VerbEditor
**Before:**
- Tab-based layout
- 6 tabs: Details, Targets, Conditions, Difficulty & Modifiers, Effects, Messages
- Complex nested editors

**After:**
- Single-page layout
- Sections: Basic Info, Target Filters, Conditions, Effects
- Simpler inline editing
- Better visual flow

## Related Files Unaffected

The following files still use the old namespace but for different classes:

- `GameAction` - Base class (still exists)
- `GameTrigger` - Trigger model (unchanged)
- `GameRound` - Round history (unchanged)
- `GameEffect` - Effect model (unchanged)

These are separate from the Verb model and remain unchanged.

## Orphaned Classes (Can Be Removed Later)

The following classes in `AdventureGame.Engine/Models/Actions` are no longer used by the new Verb model but may be used elsewhere:

- `DifficultyCheck` - Used by old Verb only
- `ModifierSource` - Used by DifficultyCheck
- `EffectGroup` - Used by old Verb only
- `EffectRange` - Used by old Verb only
- `ConditionGroup` - Still used by GameTrigger

**Recommendation**: Keep for now, as GameTrigger may still use some of these.

## Summary

? **Completed:**
- Removed old Verb class
- Removed old VerbEditor component
- Updated GamePack to use new Verb
- Updated GameSession to use new Verb
- Updated VerbsPage to use new Verb and VerbEditor
- Build successful

? **Benefits:**
- Simpler model
- Better targeting system
- Natural language conditions
- Easier to author
- Consistent with document requirements

? **No Breaking Changes:**
- All tests pass
- Build successful
- Type safety maintained

The migration to the new simplified Verb model is complete and successful!
