# Element Reference Resolution System

## Overview

This system allows game authors to reference elements using **names**, **aliases**, or **special variables** instead of hard-coded IDs. IDs are automatically resolved at runtime from the session and round context.

## Special Variables

The following special variables are always available:

| Variable | Alias | Description | Context |
|----------|-------|-------------|---------|
| `target` | `target1` | First target of the current action | Round |
| `target2` | - | Second target of the current action | Round |
| `currentScene` | `scene` | The current scene the player is in | Session |
| `currentPlayer` | `player` | The player element | Session |

## Resolution Order

When an element reference is provided, it is resolved in the following order:

1. **Special Variables** - Check if it matches a special variable (`target`, `target2`, `currentScene`, `currentPlayer`)
2. **Element ID** - Try exact match against element IDs
3. **Element Name** - Case-insensitive match against element names
4. **Element Alias** - Case-insensitive search in element aliases

## Usage Examples

### In Conditions

```json
{
  "operator": "element_name_equals",
  "parameters": {
    "element": "target",           // Special variable
    "name": "Golden Key"
  }
}

{
  "operator": "element_has_tag",
  "parameters": {
    "element": "currentScene",     // Special variable
    "tag": "dangerous"
  }
}

{
  "operator": "element_state_equals",
  "parameters": {
    "element": "main_door",        // Element name
    "stateName": "locked"
  }
}

{
  "operator": "element_property_equals",
  "parameters": {
    "element": "ancient_chest",    // Element name
    "propertyName": "material",
    "value": "oak"
  }
}
```

### In Effects

```json
{
  "action": "set_element_state",
  "parameters": {
    "element": "target",           // Special variable
    "stateName": "open"
  }
}

{
  "action": "add_element_tag",
  "parameters": {
    "element": "player",           // Special variable
    "tag": "has_key"
  }
}

{
  "action": "move_player_through_exit",
  "parameters": {
    "exit": "north_door"           // Element name
  }
}

{
  "action": "set_element_name",
  "parameters": {
    "element": "target1",          // Special variable
    "name": "Opened Chest"
  }
}
```

## Implementation

### Core Resolver

**File**: `AdventureGame.Engine\Parameters\ElementReferenceResolver.cs`

```csharp
public static class ElementReferenceResolver
{
    public static GameElement? ResolveElement(
        string? reference,
        GameRound round,
        GameSession session)
    {
        // Returns the GameElement for the reference
        // or null if not found
    }
    
    public static List<ElementReference> GetAvailableReferences(
        GameRound? round,
        GameSession session)
    {
        // Returns all available references for autocomplete
    }
}
```

### Updated Components

All conditions and effects now use `ElementReferenceResolver.ResolveElement()`:

**Conditions** (7 total):
- `ElementNameEqualsCondition`
- `ElementHasAliasCondition`
- `ElementHasTagCondition`
- `ElementAttributeCompareCondition`
- `ElementPropertyEqualsCondition`
- `ElementFlagIsCondition`
- `ElementStateEqualsCondition`

**Effects** (5 total):
- `SetElementNameEffect`
- `AddElementAliasEffect`
- `AddElementTagEffect`
- `SetElementStateEffect`
- `MovePlayerThroughExitEffect`

## Benefits

? **Author-Friendly**: No need to know or remember element IDs  
? **Portable**: References work across different game packs and sessions  
? **Flexible**: Supports names, aliases, and contextual variables  
? **Type-Safe**: Compile-time checking with runtime resolution  
? **Discoverable**: `GetAvailableReferences()` provides autocomplete data  

## Example Workflow

### Creating a Verb Action

**Author writes**:
```
Condition: target has tag "lockable"
Condition: target state equals "locked"  
Condition: player has item "Golden Key"
Effect: set target state to "unlocked"
Effect: add target tag "previously_locked"
Effect: print "You unlock the {target.name} with the golden key"
```

**Runtime resolution**:
1. User types: `unlock door`
2. Parser finds `door` element, sets as `round.Target1`
3. Conditions evaluate:
   - `target` ? resolves to `door` element
   - Checks if door has tag `"lockable"` ?
   - Checks if door state is `"locked"` ?
   - Checks if player has `"Golden Key"` ?
4. Effects execute:
   - Sets door state to `"unlocked"`
   - Adds `"previously_locked"` tag to door
   - Prints customized message

## UI Integration

### Parameter Editor Enhancement

The `ParamEditor_GameElement` component can be enhanced to show available references:

```razor
<RadzenDropDown TValue="string"
                Data="@GetAvailableReferences()"
                TextProperty="DisplayName"
                ValueProperty="Value"
                GroupProperty="Category"
                AllowFiltering="true"
                Placeholder="Element reference (name, alias, or variable)..."
                Style="width:100%" />
```

This provides grouped autocomplete:
- **Special Variables**: target, target2, currentScene, currentPlayer
- **Elements by Name**: All element names
- **Element Aliases**: All aliases with source element

## Migration Notes

### No Breaking Changes

Existing code using IDs continues to work:
- Element IDs are checked in resolution order
- Backward compatibility maintained
- All existing tests pass

### Recommended Migration

For new actions, prefer semantic references:
- ? `"element": "01H2XYZ..."`  (hard-coded ID)
- ? `"element": "target"`      (special variable)
- ? `"element": "main_door"`   (element name)
- ? `"element": "entrance"`    (alias)

## Testing

### Build Status
? All files compile successfully

### Test Coverage
- Special variable resolution
- Name-based resolution
- Alias-based resolution
- Case-insensitive matching
- Null safety
- Round/Session context integration

## Future Enhancements

### Planned Features
- [ ] Editor autocomplete UI
- [ ] Reference validation warnings
- [ ] Reference documentation tooltips
- [ ] Multi-element selection (for batch effects)
- [ ] Scoped references (parent.child notation)

### Possible Extensions
- Relative references: `parent`, `container`, `owner`
- Property accessors: `{target.name}`, `{player.health}`
- Collection filters: `items_in_scene`, `npcs_with_tag`

## Summary

The Element Reference Resolution System provides a powerful, author-friendly way to reference game elements without knowing their IDs. It supports special variables for contextual references, element names for explicit references, and aliases for alternate naming.

All conditions and effects now accept these flexible references, making game authoring significantly easier and more intuitive.
